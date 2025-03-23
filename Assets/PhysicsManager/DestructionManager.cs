using Unity.Collections;
using Unity.Mathematics;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using UnityEngine;

public class DestructionManager : MonoBehaviour
{
    public struct ProjectileData
    {
        public float3 InitPosition;
        public float3 Position;
        public float3 Direction;
        public float Speed;
        public float Radius;
        public float MaxDistance;
    }

    public GameObject DebrisPrefab;
    public WorldSimulation worldSimulation;
    public PhysicsManager physicsManager;
    VoxelDataManager voxelDataManager;

    NativeHashMap<int3, NativeArray<uint>> sphereDestructionPattern;
    NativeHashMap<int3, NativeArray<uint>> sphereCrumblePattern;
    NativeList<int3> destroyedVoxels;
    NativeList<int3> crumbleVoxels;
    bool inited = false;

    void Start()
    {
        destroyedVoxels = new NativeList<int3>(16, Allocator.Persistent);
        crumbleVoxels = new NativeList<int3>(16, Allocator.Persistent);

        voxelDataManager = worldSimulation.GetVoxelDataManager();
        (
            NativeHashMap<int3, NativeArray<uint>> destructionPattern,
            NativeHashMap<int3, NativeArray<uint>> crumblePattern
        ) = GeneratePatterns(new int3(1, 1, 1), 4, new float3(0, 1, 0));

        sphereDestructionPattern = destructionPattern;
        sphereCrumblePattern = crumblePattern;

        inited = true;
    }

    void Update()
    {

    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying | !inited)
        {
            return;
        }

        Gizmos.color = Color.green;
        for (int i = 0; i < destroyedVoxels.Length; i++)
        {
            Gizmos.DrawWireCube((float3)destroyedVoxels[i] + 0.5f, new float3(1, 1, 1));
        }

        Gizmos.color = Color.yellow;
        for (int i = 0; i < crumbleVoxels.Length; i++)
        {
            Gizmos.DrawWireCube((float3)crumbleVoxels[i] + 0.5f, new float3(1, 1, 1));
        }
    }

    void OnDestroy()
    {
        foreach (var kvp in sphereDestructionPattern)
        {
            int3 chunkPosition = kvp.Key;
            NativeArray<uint> state = kvp.Value;

            state.Dispose();
        }

        foreach (var kvp in sphereCrumblePattern)
        {
            int3 chunkPosition = kvp.Key;
            NativeArray<uint> state = kvp.Value;

            state.Dispose();
        }

        sphereDestructionPattern.Dispose();
        sphereCrumblePattern.Dispose();
        destroyedVoxels.Dispose();
        crumbleVoxels.Dispose();
    }

    public void HandleProjectileHit(ProjectileData data)
    {
        // voxelDataManager.UpdateSingleVoxel(data.Position, false, 0);

        NativeList<int3> debris = new NativeList<int3>(64, Allocator.Persistent);

        NativeHashSet<int3> dirtyChunks = new NativeHashSet<int3>(8, Allocator.Persistent);
        Matrix4x4 matrix = voxelDataManager.GetWorldToLocalMatrix();
        for (int i = 0; i < destroyedVoxels.Length; i++)
        {
            int3 offset = destroyedVoxels[i] - new int3(15, 15, 15);
            int3 localPosition = (int3)math.floor(matrix.MultiplyPoint(data.Position)) + offset;

            if (voxelDataManager.UpdateSingleVoxel(localPosition, false, 0u))
            {
                debris.Add(localPosition);
            }

            (int3 chunkPosition, int3 _)
            = voxelDataManager.GetChunkAndLocalPosition(localPosition);
            dirtyChunks.Add(chunkPosition);
        }

        for (int i = 0; i < crumbleVoxels.Length; i++)
        {
            int3 offset = crumbleVoxels[i] - new int3(15, 15, 15);
            int3 localPosition = (int3)math.floor(matrix.MultiplyPoint(data.Position)) + offset;

            voxelDataManager.UpdateSingleVoxel(localPosition, false, 0u);

            (int3 chunkPosition, int3 _)
            = voxelDataManager.GetChunkAndLocalPosition(localPosition);
            dirtyChunks.Add(chunkPosition);
        }

        foreach (int3 chunk in dirtyChunks)
        {
            voxelDataManager.AddDirtyFlag(chunk);
        }

        dirtyChunks.Dispose();

        GenerateDebris(debris);
    }

    public VoxelDataManager GetVoxelDataManager()
    {
        return voxelDataManager;
    }

    public static float SphereSDF(int3 point, int3 center, float radius)
    {
        return math.length(point - center) - radius;
    }

    public NativeHashMap<int3, NativeArray<uint>> GenerateBoundingBox(int3 chunkSize)
    {
        int size = chunkSize.x * chunkSize.y * chunkSize.z;
        const int STATE_SIZE = 32 * 32;

        NativeHashMap<int3, NativeArray<uint>> chunks = new NativeHashMap<int3, NativeArray<uint>>(
            size,
            Allocator.Persistent
        );

        for (int x = 0; x < chunkSize.x; x++)
        {
            for (int y = 0; y < chunkSize.y; y++)
            {
                for (int z = 0; z < chunkSize.z; z++)
                {
                    chunks.Add(new int3(x, y, z), new NativeArray<uint>(STATE_SIZE, Allocator.Persistent));
                }
            }
        }

        return chunks;
    }

    public void ApplySphereSDF(
        NativeArray<uint> state,
        int3 chunkPosition,
        int3 center, float
        radius
    )
    {
        for (int x = 0; x < 32; x++)
        {
            for (int y = 0; y < 32; y++)
            {
                for (int z = 0; z < 32; z++)
                {
                    int index = y + 32 * z;
                    uint mask = 1u << x;
                    if (SphereSDF(new int3(x, y, z) + chunkPosition * 32, center, radius) <= 0)
                    {
                        state[index] &= ~mask;
                        destroyedVoxels.Add(new int3(x, y, z) + chunkPosition * 32);
                    }
                    else
                    {
                        state[index] |= mask;
                    }
                }
            }
        }
    }

    public NativeHashMap<int3, NativeArray<uint>> GenerateSphereDestructionPattern(
        int3 chunkSize,
        float radius
    )
    {
        int3 size = chunkSize * 32;
        int3 center = (int3)math.floor((float3)size / 2);
        NativeHashMap<int3, NativeArray<uint>> chunks = GenerateBoundingBox(chunkSize);

        foreach (var kvp in chunks)
        {
            int3 chunkPosition = kvp.Key;
            NativeArray<uint> state = kvp.Value;

            ApplySphereSDF(state, chunkPosition, center, radius);
        }

        return chunks;
    }

    public void ApplyCrumbleSphereSDF(
        NativeArray<uint> destruction,
        NativeArray<uint> crumble,
        int3 chunkPosition,
        int3 center,
        float radius
    )
    {
        for (int x = 0; x < 32; x++)
        {
            for (int y = 0; y < 32; y++)
            {
                for (int z = 0; z < 32; z++)
                {
                    int index = y + 32 * z;
                    uint mask = 1u << x;
                    if (SphereSDF(new int3(x, y, z) + chunkPosition * 32, center, radius) <= 0)
                    {
                        if ((destruction[index] & mask) != 0)
                        {
                            crumble[index] |= mask;
                            crumbleVoxels.Add(new int3(x, y, z) + chunkPosition * 32);
                        }
                    }
                }
            }
        }
    }

    public NativeHashMap<int3, NativeArray<uint>> GenerateSphereCrumblePattern(
        NativeHashMap<int3, NativeArray<uint>> destructionPattern,
        int3 chunkSize,
        float3 centerOffset,
        float radius
    )
    {
        int3 size = chunkSize * 32;
        int3 center = (int3)math.floor((float3)size / 2 + centerOffset);
        NativeHashMap<int3, NativeArray<uint>> crumblePattern = GenerateBoundingBox(chunkSize);

        NativeArray<int3> chunkPositions = destructionPattern.GetKeyArray(Allocator.Persistent);

        for (int i = 0; i < chunkPositions.Length; i++)
        {
            NativeArray<uint> destroyedState = destructionPattern[chunkPositions[i]];
            NativeArray<uint> crumbleState = crumblePattern[chunkPositions[i]];

            ApplyCrumbleSphereSDF(destroyedState, crumbleState, chunkPositions[i], center, radius);
        }

        return crumblePattern;
    }

    (
        NativeHashMap<int3, NativeArray<uint>> destructionPattern,
        NativeHashMap<int3, NativeArray<uint>> crumblePattern
    )
    GeneratePatterns(int3 chunkSize, float radius, float3 centerOffset)
    {
        NativeHashMap<int3, NativeArray<uint>> destructionPattern
        = GenerateSphereDestructionPattern(chunkSize, radius);

        NativeHashMap<int3, NativeArray<uint>> crumblePattern
        = GenerateSphereCrumblePattern(destructionPattern, chunkSize, centerOffset, radius);

        return (destructionPattern, crumblePattern);
    }

    void GenerateDebris(NativeList<int3> debris)
    {
        Matrix4x4 matrix = voxelDataManager.GetWorldToLocalMatrix();
        for (int i = 0; i < debris.Length;)
        {
            float3 worldPosition = matrix.inverse.MultiplyPoint((float3)debris[i] + 0.5f);
            GameObject debrisObject = Instantiate(DebrisPrefab);
            debrisObject.GetComponent<DebrisCollision>().PhysicsManager = physicsManager;

            debrisObject.transform.position = worldPosition;
            debrisObject.transform.localScale = Vector3.one * 0.25f;

            i += UnityEngine.Random.Range(0, 16);
        }
    }
}
