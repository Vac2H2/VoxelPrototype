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
		QuadInstanceManager quadInstanceManager
	)
	{
		int3 chunkPosition = int3.zero;

		voxelDataManager.AddChunk(chunkPosition);
		quadInstanceManager.AddChunk(chunkPosition);

		voxelDataManager.FillChunk(chunkPosition, 1u);

		voxelDataManager.AddDirtyFlag(chunkPosition);
	}
}