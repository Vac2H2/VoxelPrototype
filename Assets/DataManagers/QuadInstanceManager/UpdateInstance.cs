using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

public abstract partial class QuadInstanceManager
{
	public void UpdateInstance(int3 chunkPosition, NativeList<VoxelQuadsGeneration.InstanceData> data)
	{
		// could not find associate chunk
		if (!chunkToBlocksMap.TryGetValue(chunkPosition, out NativeList<int> blockIndexList))
		{
			return;
		}

		// for buffer update
		int3[] chunkPositionArray = new int3[1];
		chunkPositionArray[0] = chunkPosition;

		// number of block needed to store new instances
		int numBlockNeeded = (int)math.ceil((float)data.Length / BLOCK_SIZE);

		// number of block has been used for this chunk
		int numBlockUsed = blockIndexList.Length;

		// extra block needed
		int extraBlockNeeded = numBlockNeeded - numBlockUsed;

		// block needed smaller previous -> update -> recycle
		if (extraBlockNeeded < 0)
		{
			NativeList<int> newblockIndexList = new NativeList<int>(numBlockNeeded, Allocator.Persistent);
			NativeArray<VoxelQuadsGeneration.InstanceData> dataArray = data.AsArray();
			int elementLeft = dataArray.Length;

			// use existed block
			int[] instanceCountArray = new int[1];
			for (int i = 0; i < numBlockNeeded; i++)
			{
				int instanceCount = math.min(elementLeft, BLOCK_SIZE);
				int blockIndex = blockIndexList[i];
				instanceBuffer.SetData(
					dataArray,
					i * BLOCK_SIZE,
					blockIndex * BLOCK_SIZE,
					instanceCount
				);
				chunkPositionBuffer.SetData(chunkPositionArray, 0, blockIndex, 1);
				newblockIndexList.Add(blockIndex);

				instanceCountArray[0] = instanceCount;
				instanceCountBuffer.SetData(instanceCountArray, 0, blockIndex, 1);

				elementLeft -= BLOCK_SIZE;
			}

			// free up blocks
			instanceCountArray[0] = 0; // set the block instance number to 0 avoid rendering;
			for (int i = numBlockNeeded; i < numBlockUsed; i++)
			{
				int blockIndex = blockIndexList[i];

				// back to pool
				availableBlockIndex.Enqueue(blockIndex);

				// set the block instance number to 0 avoid rendering;
				instanceCountBuffer.SetData(instanceCountArray, 0, blockIndex, 1);
			}

			// replace old block index list with new one
			blockIndexList.Dispose();
			chunkToBlocksMap[chunkPosition] = newblockIndexList;
		}
		else // block needed bigger previous -> use extra block for update
			 // or the block needed are same -> normal update
		{
			NativeArray<VoxelQuadsGeneration.InstanceData> dataArray = data.AsArray();
			int elementLeft = dataArray.Length;

			// use existed block
			int[] instanceCountArray = new int[1];
			for (int i = 0; i < numBlockNeeded; i++)
			{
				int instanceCount = math.min(elementLeft, BLOCK_SIZE);
				int blockIndex = blockIndexList[i];
				instanceBuffer.SetData(
					dataArray,
					i * BLOCK_SIZE,
					blockIndex * BLOCK_SIZE,
					instanceCount
				);
				chunkPositionBuffer.SetData(chunkPositionArray, 0, blockIndex, 1);

				instanceCountArray[0] = instanceCount;
				instanceCountBuffer.SetData(instanceCountArray, 0, blockIndex, 1);

				elementLeft -= BLOCK_SIZE;
			}

			// use available block
			for (int i = 0; i < extraBlockNeeded; i++)
			{
				int instanceCount = math.min(elementLeft, BLOCK_SIZE);
				int blockIndex = availableBlockIndex.Dequeue();
				instanceBuffer.SetData(
					dataArray,
					i * BLOCK_SIZE,
					blockIndex * BLOCK_SIZE,
					math.min(elementLeft, BLOCK_SIZE)
				);
				chunkPositionBuffer.SetData(chunkPositionArray, 0, blockIndex, 1);
				blockIndexList.Add(blockIndex);

				// update instance number in the block
				instanceCountArray[0] = instanceCount;
				instanceCountBuffer.SetData(instanceCountArray, 0, blockIndex, 1);

				elementLeft -= BLOCK_SIZE;
			}
		}
	}
}