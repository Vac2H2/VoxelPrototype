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
        float3 velocity = Protagonist.GetComponent<ProtagonistControl>().velocity;
        velocity.y += -9.81f * Time.deltaTime; // gravity

        (float3 nextVelocity, Bounds nextBounds) = ResolveCollisions(velocity, bounds);
        float3 displacement = nextBounds.center - bounds.center;
        Protagonist.transform.position += (Vector3)displacement;

        // update velocity
        Protagonist.GetComponent<ProtagonistControl>().velocity = nextVelocity;
    }
}
