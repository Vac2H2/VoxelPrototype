using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

public abstract partial class VoxelDataManager
{
	public void AddRectangle(
		int3 worldPosition,
		int3 scale
	)
	{
		(int3 chunkPosition, int3 localPosition) = GetChunkAndLocalPosition(worldPosition);
		if (!chunkMap.TryGetValue(chunkPosition, out int chunkIndex))
		{
			AddChunk(chunkPosition);
		}

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

		AddDirtyFlag(chunkPosition);
	}
}