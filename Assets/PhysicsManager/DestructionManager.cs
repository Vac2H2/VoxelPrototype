using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class DestructionManager : MonoBehaviour
{
    public struct ProjectileData
    {
        public float3 Position;
        public float3 Direction;
        public float Speed;
        public float Radius;
    }

    VoxelDataManager voxelDataManager;

    void Start()
    {
        voxelDataManager = GetComponentInParent<WorldSimulation>().GetVoxelDataManager();
    }

    void Update()
    {

    }

    public void HandleProjectileHit(ProjectileData data)
    {
        voxelDataManager.UpdateSingleVoxel(data.Position, false, 0);
    }

    public VoxelDataManager GetVoxelDataManager()
    {
        return voxelDataManager;
    }
}
