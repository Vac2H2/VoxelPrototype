using NUnit.Framework.Constraints;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public partial class PhysicsManager : MonoBehaviour
{
	public Bounds ResolveCollisions(Rigidbody rigidbody, Bounds bounds)
	{
		bounds = ResolveCollisionY(rigidbody, bounds);
		// bounds = ResolveCollisionX(speed, bounds);
		// bounds = ResolveCollisionZ(speed, bounds);

		return bounds;
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

	public Bounds ResolveCollisionY(Rigidbody rigidbody, Bounds bounds)
	{
		float3 speed = rigidbody.linearVelocity;

		Bounds oldBounds = bounds;
		Bounds adjustedBounds = oldBounds;

		float displacementY = speed.y * Time.deltaTime;
		bounds.center += new Vector3(0, displacementY, 0);

		NativeArray<Bounds> voxelBounds = GetCollidedVoxels(bounds);

		if (voxelBounds.Length > 0)
		{
			(Bounds highest, Bounds lowest) = SortBoundsInY(voxelBounds);

			if (lowest.center.y > oldBounds.center.y)
			{
				float availableRoom = lowest.min.y - oldBounds.max.y;
				adjustedBounds.center = oldBounds.center + new Vector3(0, availableRoom, 0);

				speed.y = 0;
				rigidbody.linearVelocity = speed;
			}
			if (highest.center.y < oldBounds.center.y)
			{
				float availableRoom = oldBounds.min.y - highest.max.y;
				adjustedBounds.center = oldBounds.center + new Vector3(0, -availableRoom, 0);

				speed.y = 0;
				rigidbody.linearVelocity = speed;
			}
		}
		else
		{
			adjustedBounds = bounds; // next position
		}

		voxelBounds.Dispose();
		return adjustedBounds;
	}

	public Bounds ResolveCollisionX(float3 speed, Bounds bounds)
	{
		if (speed.x == 0)
		{
			return bounds;
		}

		Bounds oldBounds = bounds;
		bounds.center += (Vector3)(speed * Time.deltaTime);
		NativeArray<Bounds> voxelBounds = GetCollidedVoxels(bounds);

		if (voxelBounds.Length > 0)
		{
			(Bounds highest, Bounds lowest) = SortBoundsInX(voxelBounds);
			if (speed.x > 0)
			{
				float availableRoom = lowest.min.x - oldBounds.max.x;
				oldBounds.center += new Vector3(availableRoom, 0, 0);
				return oldBounds;
			}
			else
			{
				float availableRoom = oldBounds.min.x - highest.max.x;
				oldBounds.center += new Vector3(-availableRoom, 0, 0);
				return oldBounds;
			}
		}
		else
		{
			return bounds;
		}
	}
	
	public Bounds ResolveCollisionZ(float3 speed, Bounds bounds)
	{
		if (speed.z == 0)
		{
			return bounds;
		}

		Bounds oldBounds = bounds;
		bounds.center += (Vector3)(speed * Time.deltaTime);
		NativeArray<Bounds> voxelBounds = GetCollidedVoxels(bounds);

		if (voxelBounds.Length > 0)
		{
			(Bounds highest, Bounds lowest) = SortBoundsInZ(voxelBounds);
			if (speed.z > 0)
			{
				float availableRoom = lowest.min.z - oldBounds.max.z;
				oldBounds.center += new Vector3(0, 0, availableRoom);
				return oldBounds;
			}
			else
			{
				float availableRoom = oldBounds.min.z - highest.max.z;
				oldBounds.center += new Vector3(0, 0, -availableRoom);
				return oldBounds;
			}
		}
		else
		{
			return bounds;
		}
	}
}