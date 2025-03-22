using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

public abstract partial class VoxelDataManager
{
	public bool CheckPositionIsSolid(float3 worldPosition)
	{
		worldPosition = worldToLocal.MultiplyPoint(worldPosition);

		int3 discretePosition = (int3)math.floor(worldPosition);
		(int3 chunkPosition, int3 localPosition) = GetChunkAndLocalPosition(discretePosition);

		(bool found, NativeSlice<uint> state, NativeSlice<uint> type) = GetChunkSlice(chunkPosition);
		if (found)
		{
			uint mask = 1u << localPosition.x;
			return (state[localPosition.y + 32 * localPosition.z] & mask) != 0;
		}

		return false;
	}

	public void UpdateSingleVoxel(float3 worldPosition, bool isAdd, uint blockType)
	{
		worldPosition = worldToLocal.MultiplyPoint(worldPosition);

		int3 discretePosition = (int3)math.floor(worldPosition);
		(int3 chunkPosition, int3 localPosition) = GetChunkAndLocalPosition(discretePosition);

		(bool found, NativeSlice<uint> state, NativeSlice<uint> type) = GetChunkSlice(chunkPosition);
		if (!found)
		{
			return;
		}

		uint mask = 1u << localPosition.x;
		if (isAdd)
		{
			state[localPosition.y + 32 * localPosition.z] |= mask;
			type[localPosition.x + 32 * localPosition.y + 32 * 32 * localPosition.z] = blockType;
		}
		else
		{
			state[localPosition.y + 32 * localPosition.z] &= ~mask;
		}

		AddDirtyFlag(chunkPosition);
	}

	public void UpdateSingleVoxel(int3 discretePosition, bool isAdd, uint blockType)
	{
		(int3 chunkPosition, int3 localPosition) = GetChunkAndLocalPosition(discretePosition);

		(bool found, NativeSlice<uint> state, NativeSlice<uint> type) = GetChunkSlice(chunkPosition);
		if (!found)
		{
			return;
		}

		uint mask = 1u << localPosition.x;
		if (isAdd)
		{
			state[localPosition.y + 32 * localPosition.z] |= mask;
			type[localPosition.x + 32 * localPosition.y + 32 * 32 * localPosition.z] = blockType;
		}
		else
		{
			state[localPosition.y + 32 * localPosition.z] &= ~mask;
		}

		AddDirtyFlag(chunkPosition);
	}

	public void UpdateStateByRangesIndependent(
		NativeSlice<uint> state,
		NativeArray<uint> independentState,
		int3 independentStart,
		int2 rangeX,
		int2 rangeY,
		int2 rangeZ
	)
	{
		// Debug.Log($"rangeX {rangeX}");
		// Debug.Log($"rangeY {rangeY}");
		// Debug.Log($"rangeZ {rangeZ}");
		for (int y = rangeY.x; y < rangeY.y; y++)
		{
			for (int z = rangeZ.x; z < rangeZ.y; z++)
			{
				uint stateData = state[y + 32 * z];

				uint independentData
				= independentState[(independentStart.y + y - rangeY.x) + 32 * (independentStart.z + z - rangeZ.x)];

				independentData >>= independentStart.x;
				independentData <<= rangeX.x;

				uint mask = GenerateMaskByRange(rangeX);
				stateData &= mask;

				Debug.Log(stateData & independentData);

				state[y + 32 * z] |= stateData & independentData;
			}
		}
	}

	public void UpdateStateByIndependentState(Bounds bounds, NativeArray<uint> independentState)
	{
		(
			int3 startChunkPosition,
			int3 endChunkPosition,
			NativeList<int2> rangeX,
			NativeList<int2> rangeY,
			NativeList<int2> rangeZ
		) = GetAABBEnclosedChunkRanges(bounds);

		int3 independentStart = int3.zero;
		for (int x = startChunkPosition.x; x < endChunkPosition.x; x++)
		{
			for (int y = startChunkPosition.y; y < endChunkPosition.y; y++)
			{
				for (int z = startChunkPosition.z; z < endChunkPosition.z; z++)
				{
					(bool found, NativeSlice<uint> state, NativeSlice<uint> type)
					= GetChunkSlice(new int3(x, y, z));

					if (!found)
					{
						independentStart.z += rangeZ[z - startChunkPosition.z].y - rangeZ[z - startChunkPosition.z].x;
						continue;
					}

					// Debug.Log($"{x} {y} {z}");
					// Debug.Log(independentStart);
					UpdateStateByRangesIndependent(
						state,
						independentState,
						independentStart,
						rangeX[x - startChunkPosition.x],
						rangeY[y - startChunkPosition.y],
						rangeZ[z - startChunkPosition.z]
					);

					independentStart.z += rangeZ[z - startChunkPosition.z].y - rangeZ[z - startChunkPosition.z].x;

					// Debug.Log($"{x} {y} {z}");
					AddDirtyFlag(new int3(x, y, z));
				}
				independentStart.y += rangeY[y - startChunkPosition.y].y - rangeY[y - startChunkPosition.y].x;
			}
			independentStart.x += rangeX[x - startChunkPosition.x].y - rangeX[x - startChunkPosition.x].x;
		}
	}
}