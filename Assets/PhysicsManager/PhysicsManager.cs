using NUnit.Framework.Constraints;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public partial class PhysicsManager : MonoBehaviour
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
        Bounds bounds = Protagonist.GetComponent<BoxCollider>().bounds;
        Rigidbody rb = Protagonist.GetComponent<Rigidbody>();

        Bounds nextBounds = ResolveCollisions(rb, bounds);
        rb.MovePosition(nextBounds.center);
        // Protagonist.GetComponent<ProtagonistControl>().UpdateCharacterState(state);
    }
}
