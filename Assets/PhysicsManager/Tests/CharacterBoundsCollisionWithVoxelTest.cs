using UnityEngine;
using Unity.Collections;

public class CharacterBoundsCollisionWithVoxelTest : MonoBehaviour
{
    public GameObject PhysicsManagerObject;
    PhysicsManager physicsManager;

    Bounds bounds;
    void Start()
    {
        physicsManager = PhysicsManagerObject.GetComponent<PhysicsManager>();
        bounds = GetComponent<BoxCollider>().bounds;
    }

    void Update()
    {
        // bounds = GetComponent<BoxCollider>().bounds;
        // NativeArray<Bounds> voxelBounds = physicsManager.GetCollidedVoxels(bounds);
        // voxelBounds.Dispose();
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        bounds = GetComponent<BoxCollider>().bounds;
        NativeArray<Bounds> voxelBounds = physicsManager.GetCollidedVoxels(bounds);

        Gizmos.color = Color.green;

        foreach (var b in voxelBounds)
        {
            Gizmos.DrawWireCube(b.center, b.size);
        }

        voxelBounds.Dispose();
    }
}
