using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public partial class VoxelUpdator : MonoBehaviour
{
	public static void GetChunkAndLocalPosition(int3 worldPos, out int3 chunkPos, out int3 localPos)
	{
		const int chunkSize = 32;

		// Convert worldPos to float3 to use math.floor for proper floor division
		chunkPos = (int3)math.floor((float3)worldPos / chunkSize);
		localPos = worldPos - chunkPos * chunkSize;
	}

	public static void AddVoxel(NativeSlice<uint> state, int x, int y, int z)
	{
		uint mask = 1u << x;
		state[y + z * 32] |= mask;
	}

	public static void RemoveVoxel(NativeSlice<uint> state, int x, int y, int z)
	{
		uint mask = 1u << x;
		state[y + z * 32] &= ~mask;
	}

	public static uint ClearBitsByRange(uint slice, int2 range)
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

		return slice & ~mask;
	}

	public static uint AndBitsByRange(uint slice, int2 range)
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

		return slice & mask;
	}

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

	public static void ClearStateByRange(NativeSlice<uint> slice, int2 xRange, int2 yRange, int2 zRange)
	{
		for (int y = yRange.x; y < yRange.y; y++)
		{
			for (int z = zRange.x; z < zRange.y; z++)
			{
				uint data = slice[y + 32 * z];
				slice[y + 32 * z] = ClearBitsByRange(data, xRange);
			}
		}
	}

	public static uint OrBitsByRange(uint slice, int2 range)
	{
		int length = range.y - range.x;
		uint mask;
		if (length == 32)
		{
			mask = uint.MaxValue;
		}
		else
		{
			mask = ((1u << length) - 1u) << range.x;
		}
		return slice |= mask;
	}

	public static void AddStateByRange(NativeSlice<uint> slice, int2 xRange, int2 yRange, int2 zRange)
	{
		for (int y = yRange.x; y < yRange.y; y++)
		{
			for (int z = zRange.x; z < zRange.y; z++)
			{
				uint data = slice[y + 32 * z];
				slice[y + 32 * z] = OrBitsByRange(data, xRange);
			}
		}
	}

	public static void UpdateTypeByRange(NativeSlice<uint> slice, int2 xRange, int2 yRange, int2 zRange, uint type)
	{
		for (int x = xRange.x; x < xRange.y; x++)
		{
			for (int y = yRange.x; y < yRange.y; y++)
			{
				for (int z = zRange.x; z < zRange.y; z++)
				{
					slice[x + 32 * y + 32 * 32 * z] = type;
				}
			}
		}
	}

	public static NativeArray<uint> FillStateByRange(NativeSlice<uint> slice, int2 xRange, int2 yRange, int2 zRange)
	{
		int sizeX = xRange.y - xRange.x;
		int sizeY = yRange.y - yRange.x;
		int sizeZ = zRange.y - zRange.x;
		int totalSize = sizeY * sizeZ;
		NativeArray<uint> masks = new NativeArray<uint>(totalSize, Allocator.Persistent);
		for (int y = yRange.x; y < yRange.y; y++)
		{
			for (int z = zRange.x; z < zRange.y; z++)
			{
				int length = sizeX;

				// uint mask = ((1u << length) - 1u) << xRange.x;
				uint mask;
				if (length == 32)
				{
					mask = uint.MaxValue;
				}
				else
				{
					mask = ((1u << length) - 1u) << xRange.x;
				}

				uint filledMask = mask & ~slice[y + 32 * z]; // find AND of empty state and adding mask
				slice[y + 32 * z] |= mask;

				masks[y - yRange.x + sizeY * (z - zRange.x)] = filledMask;
			}
		}

		return masks;
	}

	public static void UpdateTypeByRange(
		NativeSlice<uint> slice,
		NativeArray<uint> masks,
		int2 xRange,
		int2 yRange,
		int2 zRange,
		uint type)
	{
		int sizeY = yRange.y - yRange.x;
		for (int y = yRange.x; y < yRange.y; y++)
		{
			for (int z = zRange.x; z < zRange.y; z++)
			{
				uint mask = masks[y - yRange.x + sizeY * (z - zRange.x)];
				for (int x = 0; x < 32; x++)
				{
					int index = x + 32 * y + 32 * 32 * z;

					uint bit = (mask >> x) & 1u;
					slice[index] = slice[index] * (1u - bit) + type * bit;
				}
			}
		}
	}
}