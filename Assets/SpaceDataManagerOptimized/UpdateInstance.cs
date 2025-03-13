using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public partial class SpaceDataManagerOptimized
{
    public void UpdateInstance(int3 chunkPosition, NativeList<VoxelQuadsGeneration.InstanceData> data)
    {
        // could not find associate chunk
        if (!chunkMap.TryGetValue(chunkPosition, out int chunkIndex))
        {
            return;
        }

        // for buffer update
        int[] chunkIndexArray = new int[1];
        chunkIndexArray[0] = chunkIndex;

        // number of block needed to store 'data'
        int numBlockNeeded = (int)math.ceil((float)data.Length / BLOCK_SIZE);

        // could not get existed data -> first time upload -> map and allocate
        if (!blockIndexMap.TryGetValue(chunkIndex, out NativeList<int> blockIndices))
        {
            // add (chunk id, block ids) map
            NativeList<int> chunkToBlockIndicesMap = new NativeList<int>(numBlockNeeded, Allocator.Persistent);
            blockIndexMap.Add(chunkIndex, chunkToBlockIndicesMap);

            // use new creat one
            blockIndices = chunkToBlockIndicesMap;
        }

        // number of block has been used for this chunk
        int numBlockUsed = blockIndices.Length;

        // extra block needed
        int extraBlockNeeded = numBlockNeeded - numBlockUsed;

        // not enough space -> expand the buffers
        if (availableBlockIndex.Count < extraBlockNeeded)
        {
            int totalBlockToExpand = extraBlockNeeded + 64;
            int numBlockExisted = instanceBuffer.count / BLOCK_SIZE;

            ComputeBuffer instanceBufferExpanded
            = GetExpandedBuffer(instanceBuffer, sizeof(uint) * 2, totalBlockToExpand * BLOCK_SIZE);
            instanceBuffer.Release();
            instanceBuffer = instanceBufferExpanded;

            ComputeBuffer chunkIndexBufferExpanded
            = GetExpandedBuffer(chunkIndexBuffer, sizeof(int), totalBlockToExpand);
            chunkIndexBuffer.Release();
            chunkIndexBuffer = chunkIndexBufferExpanded;

            ComputeBuffer instanceCountBufferExpanded
            = GetExpandedBuffer(instanceCountBuffer, sizeof(int), totalBlockToExpand);
            instanceCountBuffer.Release();
            instanceCountBuffer = instanceCountBufferExpanded;

            // add new available block indices
            for (int i = 0; i < totalBlockToExpand; i++)
            {
                availableBlockIndex.Enqueue(numBlockExisted + i);
            }
        }


        // block needed smaller previous -> update -> recycle
        if (extraBlockNeeded < 0)
        {
            NativeList<int> newblockIndices = new NativeList<int>(numBlockNeeded, Allocator.Persistent);

            NativeArray<VoxelQuadsGeneration.InstanceData> dataArray = data.AsArray();
            int elementLeft = dataArray.Length;

            // use existed block
            int[] instanceCountArray = new int[1];
            for (int i = 0; i < numBlockNeeded; i++)
            {
                int instanceCount = math.min(elementLeft, BLOCK_SIZE);
                int blockIndex = blockIndices[i];
                instanceBuffer.SetData(
                    dataArray,
                    i * BLOCK_SIZE,
                    blockIndex * BLOCK_SIZE,
                    instanceCount
                );
                chunkIndexBuffer.SetData(chunkIndexArray, 0, blockIndex, 1);
                newblockIndices.Add(blockIndex);

                instanceCountArray[0] = instanceCount;
                instanceCountBuffer.SetData(instanceCountArray, 0, blockIndex, 1);

                elementLeft -= BLOCK_SIZE;
            }

            // free up blocks
            // int[] freeBlockIndicator = new int[1];
            // freeBlockIndicator[0] = -1;
            // instanceCountArray[0] = 0; // set the block instance number to 0 avoid rendering;
            for (int i = numBlockNeeded; i < numBlockUsed; i++)
            {
                int blockIndex = blockIndices[i];
                // set chunk index to -1 -> no need to render
                // chunkIndexBuffer.SetData(freeBlockIndicator, 0, blockIndex, 1);

                // back to pool
                availableBlockIndex.Enqueue(blockIndex);

                // set the block instance number to 0 avoid rendering;
                instanceCountBuffer.SetData(instanceCountArray, 0, blockIndex, 1);
            }

            // dispose old block indices
            blockIndices.Dispose();
            blockIndexMap[chunkIndex] = newblockIndices;
        }
        else // block needed bigger previous -> use extra block for update
        // or the block needed are same -> normal update
        {
            NativeArray<VoxelQuadsGeneration.InstanceData> dataArray = data.AsArray();
            int elementLeft = dataArray.Length;

            int[] instanceCountArray = new int[1];

            // use existed block
            for (int i = 0; i < blockIndices.Length; i++)
            {
                int instanceCount = math.min(elementLeft, BLOCK_SIZE);
                int blockIndex = blockIndices[i];
                instanceBuffer.SetData(
                    dataArray,
                    i * BLOCK_SIZE,
                    blockIndex * BLOCK_SIZE,
                    instanceCount
                );
                chunkIndexBuffer.SetData(chunkIndexArray, 0, blockIndex, 1);

                // update instance number in the block
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
                chunkIndexBuffer.SetData(chunkIndexArray, 0, blockIndex, 1);
                blockIndices.Add(blockIndex);

                // update instance number in the block
                instanceCountArray[0] = instanceCount;
                instanceCountBuffer.SetData(instanceCountArray, 0, blockIndex, 1);

                elementLeft -= BLOCK_SIZE;
            }
        }
    }

    public void UpdateType(int3 chunkPos)
    {
        if (chunkMap.TryGetValue(chunkPos, out int chunkIndex))
        {
            int startIndex = chunkIndex * TYPE_SIZE;
            NativeSlice<uint> slice = new NativeSlice<uint>(typeList.AsArray(), startIndex, TYPE_SIZE);
            typeBuffer.SetData(slice.ToArray(), 0, startIndex, TYPE_SIZE);
        }
    }
}