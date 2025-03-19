using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

public abstract partial class VoxelDataManager
{
	public static void SetStateSlice(NativeSlice<uint> state, uint value)
	{
		for (int y = 0; y < 32; y++)
		{
			for (int z = 0; z < 32; z++)
			{
				state[y + 32 * z] = value;
			}
		}
	}

	public static void SetTypeSlice(NativeSlice<uint> type, uint value)
	{
		for (int x = 0; x < 32; x++)
		{
			for (int y = 0; y < 32; y++)
			{
				for (int z = 0; z < 32; z++)
				{
					type[x + 32 * y + 32 * 32 * z] = value;
				}
			}
		}
	}

	public void FillChunk(int3 chunkPosition, uint type)
	{
		(bool found, NativeSlice<uint> stateSlice, NativeSlice<uint> typeSlice) = GetChunkSlice(chunkPosition);
		if (found)
		{
			SetStateSlice(stateSlice, uint.MaxValue);
			SetTypeSlice(typeSlice, type);
		}
	}

	public (int3 chunkPosition, int3 localPosition) GetChunkAndLocalPosition(int3 worldPosition)
	{
		const int chunkSize = 32;

		// Convert worldPos to float3 to use math.floor for proper floor division
		int3 chunkPosition = (int3)math.floor((float3)worldPosition / chunkSize);
		int3 localPosition = worldPosition - chunkPosition * chunkSize;

		return (chunkPosition, localPosition);
	}
}