using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

public class Projectile : MonoBehaviour
{

	public DestructionManager.ProjectileData projectileData;
	public DestructionManager destructionManager;
	public VoxelDataManager voxelDataManager;

	void Start()
	{
		Rigidbody rb = GetComponent<Rigidbody>();
		rb.linearVelocity = projectileData.Direction * projectileData.Speed;
	}

	void Update()
	{

	}

	void FixedUpdate()
	{
		projectileData.Position = transform.position;
		if (Vector3.Distance(projectileData.InitPosition, projectileData.Position) > projectileData.MaxDistance)
		{
			Destroy(gameObject);
			return;
		}

		if (voxelDataManager.CheckPositionIsSolid(transform.position))
		{
			destructionManager.HandleProjectileHit(projectileData);
			Destroy(gameObject);
		}
	}

    // void OnDrawGizmos()
    // {
    //     Gizmos.color = Color.green;
		
	// 	int3 discretePosition = (int3)math.floor(projectileData.Position);
	// 	Gizmos.DrawWireCube((float3)discretePosition + 0.5f, Vector3.one * 0.25f);
        
    // }
}