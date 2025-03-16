using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

public abstract partial class VoxelDataManager
{
	public static NativeList<int2> GetChunkRanges(int x, int totalLength)
	{
		NativeList<int2> ranges = new NativeList<int2>(10, Allocator.Persistent);
		int firstChunkLength = 32 - x;

		if (totalLength <= firstChunkLength)
		{
			ranges.Add(new int2(x, x + totalLength));
		}
		else
		{
			ranges.Add(new int2(x, 32));
			totalLength -= firstChunkLength;
		}

		while (totalLength >= 32)
		{
			ranges.Add(new int2(0, 32));
			totalLength -= 32;
		}

		if (totalLength > 0)
		{
			ranges.Add(new int2(0, totalLength));
		}

		return ranges;
	}

	/// <summary>
	/// Input a bounds, get overlapped chunks. For each chunk,
	/// get a range inside the chunk.
	/// </summary>
	/// <param name="bounds">A bounds, could be character's AABB</param>
	/// <returns></returns>
	public (
		int3 startChunkPosition,
		int3 endChunkPosition,
		NativeList<int2> rangeX,
		NativeList<int2> rangeY,
		NativeList<int2> rangeZ
	) GetAABBEnclosedChunkRanges(Bounds bounds)
	{
		int3 start = (int3)math.floor((float3)bounds.min);
		int3 end = (int3)math.floor((float3)bounds.max);
		int3 scale = start - end + 1;

		(int3 startChunkPosition, int3 startLocalPosition) = GetChunkAndLocalPosition(start);
		(int3 endChunkPosition, int3 _) = GetChunkAndLocalPosition(end);
		NativeList<int2> rangeX = GetChunkRanges(startLocalPosition.x, scale.x);
		NativeList<int2> rangeY = GetChunkRanges(startLocalPosition.y, scale.y);
		NativeList<int2> rangeZ = GetChunkRanges(startLocalPosition.z, scale.z);

		return (startChunkPosition, endChunkPosition, rangeX, rangeY, rangeZ);
	}

	public static uint GenerateMaskByRange(int2 range)
	{
		int length = range.y - range.x;
		// uint mask = ((1u << length) - 1u) << range.x;
		uint mask;
		if (length == 32)
		{
			mask = uint.MaxValue;
		}
		else
		{
			mask = ((1u << length) - 1u) << range.x;
		}

		return mask;
	}

	public NativeList<int3> GetSolidVoxelsInRangeOfChunk(
		NativeSlice<uint> slice,
		int2 rangeX,
		int2 rangeY,
		int2 rangeZ
	)
	{
		NativeList<int3> voxels = new NativeList<int3>(10, Allocator.Persistent);
		for (int y = rangeY.x; y < rangeY.y; y++)
		{
			for (int z = rangeZ.x; z < rangeZ.y; z++)
			{
				uint data = slice[y + 32 * z];
				uint mask = GenerateMaskByRange(rangeX);
				uint result = data & mask;

				while (result != 0)
				{
					int x = math.tzcnt(result);

					// Clear the lowest set bit
					result &= result - 1;

					voxels.Add(new int3(x, y, z));
				}
			}
		}

		return voxels;
	}

	/// <summary>
	/// Given a bounds, find all solid voxels inside the bounds
	/// </summary>
	/// <param name="bounds">A bounds in object(local) space.
	/// Commonly, you need to transform a bounds from world space
	/// into voxel data manager's local space.
	/// </param>
	/// <returns></returns>
	public NativeList<int3> GetSolidVoxelsWithinBounds(Bounds bounds)
	{
		(
			int3 startChunkPosition,
			int3 endChunkPosition,
			NativeList<int2> rangeX,
			NativeList<int2> rangeY,
			NativeList<int2> rangeZ
		) = GetAABBEnclosedChunkRanges(bounds);

		NativeList<int3> allVoxels = new NativeList<int3>(24, Allocator.Persistent);
		for (int x = startChunkPosition.x; x < endChunkPosition.x + 1; x++)
		{
			for (int y = startChunkPosition.y; y < endChunkPosition.y + 1; y++)
			{
				for (int z = startChunkPosition.z; z < endChunkPosition.z + 1; z++)
				{
					(bool found, NativeSlice<uint> state, NativeSlice<uint> _)
					= GetChunkSlice(new int3(x, y, z));

					if (!found)
					{
						continue;
					}

					NativeList<int3> voxels = GetSolidVoxelsInRangeOfChunk(
						state,
						rangeX[x],
						rangeY[y],
						rangeZ[z]
					);

					allVoxels.AddRange(voxels.AsArray());
					voxels.Dispose();
				}
			}
		}

		return allVoxels;
	}

	/// <summary>
	/// Generate a bounds in voxel space.
	/// </summary>
	/// <param name="worldPosition">World position</param>
	/// <param name="voxelSpaceScale">Scales in voxel space. For example, a character width of 2 world voxel</param>
	/// <returns></returns>
	public Bounds GenerateVoxelSpaceBounds(float3 worldPosition, float3 voxelSpaceScale)
	{
		Vector3 localPosition = worldToLocal.MultiplyPoint3x4(worldPosition);
		Bounds bounds = new Bounds(localPosition, voxelSpaceScale);

		return bounds;
	}

	/// <summary>
	/// Given voxels in voxel space positions, generate bounds in world space.
	/// </summary>
	/// <param name="voxels">Voxel space voxel positions</param>
	/// <returns></returns>
	public NativeArray<Bounds> ConvertVoxelsIntoBounds(
		NativeList<int3> voxels
	)
	{
		NativeArray<Bounds> voxelBounds = new NativeArray<Bounds>(voxels.Length, Allocator.Persistent);
		Vector3 worldScale = new Vector3(
			localToWorld.GetColumn(0).magnitude,
			localToWorld.GetColumn(1).magnitude,
			localToWorld.GetColumn(2).magnitude
		);

		for (int i = 0; i < voxels.Length; i++)
		{
			float3 localPosition = (float3)voxels[i] + 0.5f;
			Vector3 worldPosition = localToWorld.MultiplyPoint3x4(localPosition);
			Bounds bounds = new Bounds(worldPosition, worldScale);
			voxelBounds[i] = bounds;
		}

		return voxelBounds;
	}
}