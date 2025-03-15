using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

public static partial class VoxelDataUpdator
{
	// public static void AddVoxel(VoxelDataManager voxelDataManager, int3 position)
	// {

	// }

	public static void CreateBasicScene(
		VoxelDataManager voxelDataManager,
		QuadInstanceManager quadInstanceManager,
		int3 start,
		int3 scale
	)
	{
		int3 end = start + scale;
		for (int x = start.x; x < end.x; x++)
		{
			for (int y = start.y; y < end.y; y++)
			{
				for (int z = start.z; z < end.z; z++)
				{
					int3 chunkPosition = new int3(x, y, z);

					voxelDataManager.AddChunk(chunkPosition);
					quadInstanceManager.AddChunk(chunkPosition);

					voxelDataManager.FillChunk(chunkPosition, 1u);

					voxelDataManager.AddDirtyFlag(chunkPosition);
				}
			}
		}
	}
}