using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class PhysicsManager : MonoBehaviour
{
    public Transform Protagonist;
    public Transform WorldSimulation;
    VoxelDataManager voxelDataManager;
    void Start()
    {
        voxelDataManager = WorldSimulation.GetComponent<WorldSimulation>().GetVoxelDataManager();
    }

    void Update()
    {
        CharacterState state = Protagonist.GetComponent<ProtagonistControl>().GetCharacterState();

        // Protagonist.transform.position += (Vector3)(state.Speed * Time.deltaTime);
    }

    /// <summary>
    /// Given world space bounds get all collided voxels represented by bounds
    /// </summary>
    /// <param name="position">World position</param>
    /// <param name="scale">Scales in voxel space. For example, a character width of 2 world voxel</param>
    /// <returns></returns>
    public NativeArray<Bounds> GetCollidedVoxels(float3 position, float3 scale)
    {
        Bounds bounds = voxelDataManager.GenerateVoxelSpaceBounds(position, scale);
        NativeList<int3> voxels = voxelDataManager.GetSolidVoxelsWithinBounds(bounds);
        NativeArray<Bounds> boundsArray = voxelDataManager.ConvertVoxelsIntoBounds(voxels);

        return boundsArray;
    } 
}
