using NUnit.Framework.Constraints;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public partial class PhysicsManager : MonoBehaviour
{
	public (float3 nextVelocity, Bounds nextBounds) ResolveCollisions(float3 velocity, Bounds bounds)
	{
		Bounds nextBounds;
		float3 nextVelocity;
		(nextVelocity, nextBounds) = ResolveCollisionY(velocity, bounds);
		(nextVelocity, nextBounds) = ResolveCollisionX(nextVelocity, nextBounds);
		(nextVelocity, nextBounds) = ResolveCollisionZ(nextVelocity, nextBounds);

		return (nextVelocity, nextBounds);
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

	public (Bounds highest, Bounds lowest) SortBoundsInY(NativeArray<Bounds> voxelBounds)
	{
		Bounds highest = voxelBounds[0];
		Bounds lowest = voxelBounds[0];
		for (int i = 1; i < voxelBounds.Length; i++)
		{
			float oldHighestY = highest.center.y;
			float oldLowestY = lowest.center.y;
			float newY = voxelBounds[i].center.y;

			if (newY > oldHighestY)
			{
				highest = voxelBounds[i];
			}

			if (newY < oldLowestY)
			{
				lowest = voxelBounds[i];
			}
		}

		return (highest, lowest);
	}

	public (Bounds highest, Bounds lowest) SortBoundsInX(NativeArray<Bounds> voxelBounds)
	{
		Bounds highest = voxelBounds[0];
		Bounds lowest = voxelBounds[0];
		for (int i = 1; i < voxelBounds.Length; i++)
		{
			float oldHighestX = highest.center.x;
			float oldLowestX = lowest.center.x;
			float newX = voxelBounds[i].center.x;

			if (newX > oldHighestX)
			{
				highest = voxelBounds[i];
			}

			if (newX < oldLowestX)
			{
				lowest = voxelBounds[i];
			}
		}

		return (highest, lowest);
	}

	public (Bounds highest, Bounds lowest) SortBoundsInZ(NativeArray<Bounds> voxelBounds)
	{
		Bounds highest = voxelBounds[0];
		Bounds lowest = voxelBounds[0];
		for (int i = 1; i < voxelBounds.Length; i++)
		{
			float oldHighestZ = highest.center.z;
			float oldLowestZ = lowest.center.z;
			float newZ = voxelBounds[i].center.z;

			if (newZ > oldHighestZ)
			{
				highest = voxelBounds[i];
			}

			if (newZ < oldLowestZ)
			{
				lowest = voxelBounds[i];
			}
		}

		return (highest, lowest);
	}

	public (float3 updatedVelocity, Bounds updatedBounds) ResolveCollisionY(float3 velocity, Bounds bounds)
	{
		float3 speed = velocity;

		if (speed.y == 0)
		{
			return (speed, bounds);
		}

		// Generate next bounds in Y axis
		Bounds nextBounds = bounds;
		float displacementY = speed.y * Time.deltaTime;
		nextBounds.center += new Vector3(0, displacementY, 0);

		// find collided voxels in next bounds
		NativeArray<Bounds> voxelBounds = GetCollidedVoxels(nextBounds);

		if (voxelBounds.Length > 0)
		{
			(Bounds highest, Bounds lowest) = SortBoundsInY(voxelBounds);

			if (speed.y > 0)
			{
				float availableRoom = (lowest.min.y - 0.01f) - bounds.max.y;
				nextBounds.center = bounds.center + new Vector3(0, availableRoom, 0);

				speed.y = 0;
				velocity = speed;
			}
			else 
			{
				float availableRoom = bounds.min.y - (highest.max.y + 0.01f);
				nextBounds.center = bounds.center + new Vector3(0, -availableRoom, 0);

				speed.y = 0;
				velocity = speed;
			}
		}

		voxelBounds.Dispose();
		return (velocity, nextBounds);
	}

	public (bool stepped, Bounds steppedBounds) ResolveStepping(Bounds bounds, NativeArray<Bounds> voxelBounds)
	{
		bool stepped = false;
		Bounds nextBounds = bounds;

		if (voxelBounds.Length > 0)
		{
			(Bounds highest, Bounds lowest) = SortBoundsInY(voxelBounds);

			float availableRoom = highest.max.y - bounds.min.y;
			float maxStepHeight = 0.8f;
			if (maxStepHeight > availableRoom)
			{
				nextBounds.center = bounds.center + new Vector3(0, availableRoom, 0);

				NativeArray<Bounds> voxelCollisionsAfterStep = GetCollidedVoxels(nextBounds);

				if (voxelCollisionsAfterStep.Length == 0)
				{
					stepped = true;
				}
				else
				{
					// set to original
					nextBounds = bounds;
				}

				voxelCollisionsAfterStep.Dispose();
			}
		}
		return (stepped, nextBounds);
	}

	public (float3 updatedVelocity, Bounds updatedBounds) ResolveCollisionX(float3 velocity, Bounds bounds)
	{
		float3 speed = velocity;

		if (speed.x == 0)
		{
			return (speed, bounds);
		}

		// Generate next bounds in X axis
		Bounds nextBounds = bounds;
		float displacementX = speed.x * Time.deltaTime;
		nextBounds.center += new Vector3(displacementX, 0, 0);

		// find collided voxels in next bounds
		NativeArray<Bounds> voxelBounds = GetCollidedVoxels(nextBounds);

		if (voxelBounds.Length > 0)
		{
			// can step up?
			(bool stepped, Bounds steppedBounds) = ResolveStepping(nextBounds, voxelBounds);
			if (stepped)
			{
				voxelBounds.Dispose();
				return (velocity, steppedBounds);
			}
			
			(Bounds highest, Bounds lowest) = SortBoundsInX(voxelBounds);

			if (speed.x > 0)
			{
				float availableRoom = (lowest.min.x - 0.01f) - bounds.max.x;
				nextBounds.center = bounds.center + new Vector3(availableRoom, 0, 0);
			}
			else
			{
				float availableRoom = bounds.min.x - (highest.max.x + 0.01f);
				nextBounds.center = bounds.center + new Vector3(-availableRoom, 0, 0);
			}

			speed.x = 0;
			velocity = speed;
		}

		voxelBounds.Dispose();
		return (velocity, nextBounds);
	}
	
	public (float3 updatedVelocity, Bounds updatedBounds) ResolveCollisionZ(float3 velocity, Bounds bounds)
	{
		float3 speed = velocity;

		if (speed.z == 0)
		{
			return (speed, bounds);
		}

		// Generate next bounds in Y axis
		Bounds nextBounds = bounds;
		float displacementZ = speed.z * Time.deltaTime;
		nextBounds.center += new Vector3(0, 0, displacementZ);

		// find collided voxels in next bounds
		NativeArray<Bounds> voxelBounds = GetCollidedVoxels(nextBounds);

		if (voxelBounds.Length > 0)
		{
			// can step up?
			(bool stepped, Bounds steppedBounds) = ResolveStepping(nextBounds, voxelBounds);
			if (stepped)
			{
				voxelBounds.Dispose();
				return (velocity, steppedBounds);
			}

			(Bounds highest, Bounds lowest) = SortBoundsInZ(voxelBounds);

			if (speed.z > 0)
			{
				float availableRoom = (lowest.min.z - 0.01f) - bounds.max.z;
				nextBounds.center = bounds.center + new Vector3(0, 0, availableRoom);

				speed.z = 0;
				velocity = speed;
			}
			else 
			{
				float availableRoom = bounds.min.z - (highest.max.z + 0.01f);
				nextBounds.center = bounds.center + new Vector3(0, 0, -availableRoom);

				speed.z = 0;
				velocity = speed;
			}
		}

		voxelBounds.Dispose();
		return (velocity, nextBounds);
	}
}