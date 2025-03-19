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
}