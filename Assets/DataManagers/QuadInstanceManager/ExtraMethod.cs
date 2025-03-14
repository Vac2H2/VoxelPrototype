using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

public abstract partial class QuadInstanceManager
{
	public int GetNumOfAvailableBlocks()
	{
		return availableBlockIndex.Count;
	}

	public bool CheckChunkExistsInChunkToBlocksMap(int3 chunkPosition)
	{
		if (chunkToBlocksMap.ContainsKey(chunkPosition))
		{
			return true;
		}
		return false;
	}

	public static NativeList<VoxelQuadsGeneration.InstanceData> GenerateDummyInstances(
		int number,
		VoxelQuadsGeneration.InstanceData data
	)
	{
		NativeList<VoxelQuadsGeneration.InstanceData> list
		= new NativeList<VoxelQuadsGeneration.InstanceData>(
			number,
			Allocator.Persistent
		);

		for (int i = 0; i < number; i++)
		{
			list.Add(
				new VoxelQuadsGeneration.InstanceData
				{
					EncodedQuadData = data.EncodedQuadData,
					Direction = data.Direction
				}
			);
		}

		return list;
	}

	public bool CheckInstanceBufferRange(VoxelQuadsGeneration.InstanceData data, int start, int length)
	{
		VoxelQuadsGeneration.InstanceData[] bufferData
		= new VoxelQuadsGeneration.InstanceData[instanceBuffer.count];

		instanceBuffer.GetData(bufferData);

		for (int i = start; i < start + length; i++)
		{
			uint quad = bufferData[i].EncodedQuadData;
			uint direction = bufferData[i].Direction;
			if (quad != data.EncodedQuadData | direction != bufferData[i].Direction)
			{
				return false;
			}
		}
		return true;
	}

	public int GetBlockInstanceCount(int blockIndex)
	{
		int bufferSize = instanceCountBuffer.count;
		int[] instCountArray = new int[bufferSize];

		instanceCountBuffer.GetData(instCountArray);

		return instCountArray[blockIndex];
	}
}