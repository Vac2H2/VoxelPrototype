using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

public abstract partial class VoxelDataManager
{
	public void AddRectangle(
		int3 worldPosition,
		int3 scale,
		uint blockType
	)
	{
		(int3 chunkPosition, int3 localPosition) = GetChunkAndLocalPosition(worldPosition);
		(bool found, NativeSlice<uint> state, NativeSlice<uint> type) = GetChunkSlice(chunkPosition);
		int3 endLocalPosition = localPosition + scale;
		uint mask = GenerateMaskByRange(new int2(localPosition.x, endLocalPosition.x));
		for (int y = localPosition.y; y < endLocalPosition.y; y++)
		{
			for (int z = localPosition.z; z < endLocalPosition.z; z++)
			{
				state[y + 32 * z] |= mask;
			}
		}

		for (int x = localPosition.x; x < endLocalPosition.x; x++)
		{
			for (int y = localPosition.y; y < endLocalPosition.y; y++)
			{
				for (int z = localPosition.z; z < endLocalPosition.z; z++)
				{
					type[x + 32 * y + 32 * 32 * z] = blockType;
				}
			}
		}
	}
}