using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

public static partial class VoxelDataUpdator
{
	public static void CreateBasicScene(
		VoxelDataManager voxelDataManager,
		QuadInstanceManager quadInstanceManager,
		int3 start,
		int3 scale,
		uint type
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

					voxelDataManager.FillChunk(chunkPosition, type);

					voxelDataManager.AddDirtyFlag(chunkPosition);
				}
			}
		}
	}

	public static void AddRectangle(
		VoxelDataManager voxelDataManager,
		QuadInstanceManager quadInstanceManager,
		int3 worldPosition,
		int3 scale,
		uint blockType
	)
	{
		(int3 chunkPosition, int3 localPosition) = voxelDataManager.GetChunkAndLocalPosition(worldPosition);
		// Debug.Log($"chunkPosition: {chunkPosition}, localPosition: {localPosition}");
		voxelDataManager.AddChunk(chunkPosition);
		quadInstanceManager.AddChunk(chunkPosition);

		voxelDataManager.AddRectangle(worldPosition, scale, blockType);

		voxelDataManager.AddDirtyFlag(chunkPosition);
	}

	public static void AddStaircase(
		VoxelDataManager voxelDataManager,
		QuadInstanceManager quadInstanceManager,
		int3 worldPosition,
		int3 scale,
		uint blockType
	)
	{
		AddRectangle(voxelDataManager, quadInstanceManager, worldPosition, scale, blockType);
		AddRectangle(voxelDataManager, quadInstanceManager, worldPosition + new int3(1, 1, 0), scale, blockType);
		AddRectangle(voxelDataManager, quadInstanceManager, worldPosition + new int3(2, 2, 0), scale, blockType);
		AddRectangle(voxelDataManager, quadInstanceManager, worldPosition + new int3(3, 3, 0), scale, blockType);
	}
}