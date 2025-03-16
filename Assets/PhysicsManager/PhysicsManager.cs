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
        // CharacterState state = Protagonist.GetComponent<ProtagonistControl>().GetCharacterState();

        // Protagonist.transform.position += (Vector3)(state.Speed * Time.deltaTime);
    }

    /// <summary>
    /// Given world space bounds get all collided voxels represented by bounds
    /// </summary>
    /// <param name="bounds">Bounds you want to check in world space</param>
    /// <returns></returns>
    public NativeArray<Bounds> GetCollidedVoxels(Bounds bounds)
    {
        Bounds voxelSpaceBounds = voxelDataManager.ConvertBoundsToVoxelSpace(bounds);
        NativeList<int3> voxels = voxelDataManager.GetSolidVoxelsWithinBounds(voxelSpaceBounds);
        NativeArray<Bounds> boundsArray = voxelDataManager.ConvertVoxelsIntoBounds(voxels);

        voxels.Dispose();
        return boundsArray;
    } 
}
