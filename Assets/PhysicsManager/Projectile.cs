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
		if (voxelDataManager.CheckPositionIsSolid(projectileData.Position))
		{
			destructionManager.HandleProjectileHit(projectileData);
			Destroy(gameObject);
		}
    }
}