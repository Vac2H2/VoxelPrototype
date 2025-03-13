using Unity.Collections;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public partial class SpaceDataManagerOptimized
{
	public void ExtractByBox(
		int3 startPos,
		int3 endPos,
		out int3 boxScales,
		out NativeArray<uint> state,
		out NativeArray<uint> type)
	{
		int3 iterStart = int3.zero;
		int3 iterEnd = int3.zero;
		for (int i = 0; i < 3; i++)
		{
			int min = math.min(startPos[i], endPos[i]);
			int max = math.max(startPos[i], endPos[i]);
			iterStart[i] = min;
			iterEnd[i] = max;
		}

		VoxelUpdator.GetChunkAndLocalPosition(iterStart, out int3 chunkPosStart, out int3 localPosStart);
		VoxelUpdator.GetChunkAndLocalPosition(iterEnd, out int3 chunkPosEnd, out int3 localPosEnd);
		NativeList<int2> xRanges = VoxelUpdator.GetChunkRanges(localPosStart.x, iterEnd.x - iterStart.x + 1);
		NativeList<int2> yRanges = VoxelUpdator.GetChunkRanges(localPosStart.y, iterEnd.y - iterStart.y + 1);
		NativeList<int2> zRanges = VoxelUpdator.GetChunkRanges(localPosStart.z, iterEnd.z - iterStart.z + 1);

		NativeArray<int> xPrefixSums = GetPrefixSumFromRanges(xRanges);
		NativeArray<int> yPrefixSums = GetPrefixSumFromRanges(yRanges);
		NativeArray<int> zPrefixSums = GetPrefixSumFromRanges(zRanges);

		int3 chunkScales = chunkPosEnd - chunkPosStart + 1;

		boxScales = iterEnd - iterStart + 1;

		int numChunkX = chunkScales.x;
		state = new NativeArray<uint>(boxScales.y * boxScales.z * numChunkX, Allocator.Persistent);
		type = new NativeArray<uint>(boxScales.x * boxScales.y * boxScales.z, Allocator.Persistent);
		// Debug.Log($"boxScales {boxScales}");
		// Debug.Log(state.Length);
		// Debug.Log(type.Length);

		for (int x = 0; x < chunkScales.x; x++)
		{
			for (int y = 0; y < chunkScales.y; y++)
			{
				for (int z = 0; z < chunkScales.z; z++)
				{
					int3 chunkPos = chunkPosStart + new int3(x, y, z);
					if (!GetChunkStateSlice(chunkPos, out NativeSlice<uint> stateSlice))
					{
						continue;
					}
					GetChunkTypeSlice(chunkPos, out NativeSlice<uint> typeSlice);

					AddStateByRange(
						state,
						stateSlice,
						boxScales,
						xRanges[x],
						yRanges[y],
						zRanges[z],
						xPrefixSums[x],
						yPrefixSums[y],
						zPrefixSums[z]
					);
					AddTypeByRange(
						type,
						typeSlice,
						boxScales,
						xRanges[x],
						yRanges[y],
						zRanges[z],
						xPrefixSums[x],
						yPrefixSums[y],
						zPrefixSums[z]
					);
				}
			}
		}

		xPrefixSums.Dispose();
		yPrefixSums.Dispose();
		zPrefixSums.Dispose();
		xRanges.Dispose();
		yRanges.Dispose();
		zRanges.Dispose();
	}

	public static NativeArray<int> GetPrefixSumFromRanges(NativeList<int2> ranges)
	{
		NativeArray<int> prefixSums = new NativeArray<int>(ranges.Length, Allocator.Persistent);
		int sum = 0;
		prefixSums[0] = sum;
		for (int i = 1; i < ranges.Length; i++)
		{
			int2 range = ranges[i - 1];
			sum += range.y - range.x;
			prefixSums[i] = sum;
		}
		return prefixSums;
	}

	public static void AddStateByRange(
		NativeArray<uint> state,
		NativeSlice<uint> stateSlice,
		int3 scales,
		int2 xRange,
		int2 yRange,
		int2 zRange,
		int xStart,
		int yStart,
		int zStart)
	{
		int xIndex = xStart / 32;
		int numChunkX = (int)math.ceil((float)scales.x / 32);
		for (int y = yRange.x; y < yRange.y; y++)
		{
			for (int z = zRange.x; z < zRange.y; z++)
			{
				int extractorIndex
				= xIndex + numChunkX * (yStart + y - yRange.x) + numChunkX * scales.y * (zStart + z - zRange.x);

				// Debug.Log($"extractorIndex {extractorIndex}");
				// Debug.Log($"extractor value {stateSlice[y + 32 * z]}");
				state[extractorIndex] = VoxelUpdator.AndBitsByRange(stateSlice[y + 32 * z], xRange) >> xRange.x;
			}
		}
	}

	public static void AddTypeByRange(
		NativeArray<uint> type,
		NativeSlice<uint> typeSlice,
		int3 scales,
		int2 xRange,
		int2 yRange,
		int2 zRange,
		int xStart,
		int yStart,
		int zStart)
	{
		for (int x = xRange.x; x < xRange.y; x++)
		{
			for (int y = yRange.x; y < yRange.y; y++)
			{
				for (int z = zRange.x; z < zRange.y; z++)
				{
					int extractorIndex
					= xStart + (x - xRange.x)
					+ scales.x * (yStart + y - yRange.x)
					+ scales.x * scales.y * (zStart + z - zRange.x);

					type[extractorIndex] = typeSlice[x + 32 * y + 32 * 32 * z];
				}
			}
		}
	}
}