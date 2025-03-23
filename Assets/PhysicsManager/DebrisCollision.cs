using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.UIElements;

public class DebrisCollision : MonoBehaviour
{
    public PhysicsManager PhysicsManager;
    float startTime;
    float delay = 3f;
    void Start()
    {
        startTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        Bounds bounds = GetComponent<BoxCollider>().bounds;
        NativeArray<Bounds> voxelBounds = PhysicsManager.GetCollidedVoxels(
            GetComponent<BoxCollider>().bounds
        );
        Rigidbody rb = GetComponent<Rigidbody>();
        foreach (var b in voxelBounds)
        {
            (float3 nextVelocity, Bounds nextBounds)
            = PhysicsManager.ResolveCollisions(rb.linearVelocity, bounds);
            rb.MovePosition(nextBounds.center);

            Vector3 direction = (bounds.center - b.center).normalized;
            rb.linearVelocity = -direction * (rb.linearVelocity.magnitude * 0.3f);
        }
        voxelBounds.Dispose();

        if (Time.time - startTime >= delay)
        {
            Destroy(gameObject);
        }
    }
}
