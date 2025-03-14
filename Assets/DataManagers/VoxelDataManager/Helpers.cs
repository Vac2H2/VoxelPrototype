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
					type[y + 32 * z] = value;
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
}