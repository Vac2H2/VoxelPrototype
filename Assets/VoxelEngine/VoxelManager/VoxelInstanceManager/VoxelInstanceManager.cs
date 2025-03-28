using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

namespace VoxelEngine.VoxelManager
{
	/// <summary>
	/// VoxelInstanceManager stores voxel(quad) instances in GPU
	/// </summary>
	public partial class VoxelInstanceManager
	{
		public struct InstanceData
		{
			public uint EncodedQuadData;
			public uint Direction;
		}

		NativeHashMap<int3, NativeList<int>> chunkToBlocksMap;
		NativeQueue<int> availableBlockIndex;

		int maxNumChunk;
		int maxNumBlockPerChunk;
		int blockSize;
		ComputeBuffer instanceBuffer;
		ComputeBuffer instanceCountBuffer;
		ComputeBuffer chunkPositionBuffer;
		ComputeBuffer typeIndexBuffer;
		ComputeBuffer visibleBlockCountBuffer;
		ComputeBuffer visibleBlockBuffer;

		public VoxelInstanceManager(int _maxNumChunk, int _maxNumBlockPerChunk, int _blockSize)
		{
			maxNumChunk = _maxNumChunk;
			maxNumBlockPerChunk = _maxNumBlockPerChunk;
			blockSize = _blockSize;

			chunkToBlocksMap = new NativeHashMap<int3, NativeList<int>>(maxNumChunk, Allocator.Persistent);
			availableBlockIndex = new NativeQueue<int>(Allocator.Persistent);

			int totalBlocks = maxNumBlockPerChunk * maxNumChunk;
			instanceBuffer = new ComputeBuffer(
				totalBlocks * blockSize,
				sizeof(uint) * 2,
				ComputeBufferType.Structured
			);

			chunkPositionBuffer = new ComputeBuffer(
				totalBlocks,
				sizeof(int) * 3,
				ComputeBufferType.Structured
			);

			instanceCountBuffer = new ComputeBuffer(
				totalBlocks,
				sizeof(int),
				ComputeBufferType.Structured
			);

			typeIndexBuffer = new ComputeBuffer(
				totalBlocks,
				sizeof(int),
				ComputeBufferType.Structured
			);

			visibleBlockBuffer = new ComputeBuffer(
				totalBlocks,
				sizeof(int),
				ComputeBufferType.Structured
			);

			visibleBlockCountBuffer = new ComputeBuffer(
				1,
				sizeof(int),
				ComputeBufferType.Structured
			);
		}

		public void AddChunk(int3 chunkPosition)
		{
			NativeList<int> blockIndices = new NativeList<int>(4, Allocator.Persistent);
			chunkToBlocksMap.TryAdd(chunkPosition, blockIndices);
		}

		public void UpdateVisibleBlockBuffer(NativeList<int3> visibleChunks)
		{
			NativeList<int> visibleBlocks = GetVisibleGPUBlocks(visibleChunks);
			visibleBlockBuffer.SetData(visibleBlocks.AsArray());
			visibleBlockCountBuffer.SetData(new int[1] { visibleBlocks.Length });
			visibleBlocks.Dispose();
		}

		public
		(
			ComputeBuffer instanceBuffer,
			ComputeBuffer instanceCountBuffer,
			ComputeBuffer chunkPositionBuffer,
			ComputeBuffer typeIndexBuffer,
			ComputeBuffer visibleBlockBuffer,
			ComputeBuffer visibleBlockCountBuffer
		) GetBuffers()
		{
			return
			(
				instanceBuffer,
				instanceCountBuffer,
				chunkPositionBuffer,
				typeIndexBuffer,
				visibleBlockBuffer,
				visibleBlockCountBuffer
			);
		}

		public void Dispose()
		{
			NativeArray<int3> allKeys = chunkToBlocksMap.GetKeyArray(Allocator.Temp);
			for (int i = 0; i < allKeys.Length; i++)
			{
				int3 key = allKeys[i];
				NativeList<int> listForKey = chunkToBlocksMap[key];
				listForKey.Dispose();
			}
			chunkToBlocksMap.Dispose();
			availableBlockIndex.Dispose();

			instanceBuffer.Release();
			chunkPositionBuffer.Release();
			instanceCountBuffer.Release();
			typeIndexBuffer.Release();
		}

		/// <summary>
		/// Update greedy quad instances given a chunk
		/// </summary>
		/// <param name="chunkPosition">The chunk you want to update</param>
		/// <param name="data"></param>
		public void UpdateInstance(
			int3 chunkPosition,
			int chunkIndex,
			NativeList<InstanceData> data
		)
		{
			// could not find associate chunk
			if (!chunkToBlocksMap.TryGetValue(chunkPosition, out NativeList<int> blockIndexList))
			{
				return;
			}

			// for buffer update
			int3[] chunkPositionArray = new int3[1];
			chunkPositionArray[0] = chunkPosition;

			int[] chunkIndexArray = new int[1];
			chunkIndexArray[0] = chunkIndex;

			// number of block needed to store new instances
			int numBlockNeeded = (int)math.ceil((float)data.Length / blockSize);

			// number of block has been used for this chunk
			int numBlockUsed = blockIndexList.Length;

			// extra block needed
			int extraBlockNeeded = numBlockNeeded - numBlockUsed;

			// block needed smaller previous -> update -> recycle
			if (extraBlockNeeded < 0)
			{
				NativeList<int> newblockIndexList = new NativeList<int>(numBlockNeeded, Allocator.Persistent);
				NativeArray<InstanceData> dataArray = data.AsArray();
				int elementLeft = dataArray.Length;

				// use existed block
				int[] instanceCountArray = new int[1];
				for (int i = 0; i < numBlockNeeded; i++)
				{
					int instanceCount = math.min(elementLeft, blockSize);
					int blockIndex = blockIndexList[i];
					instanceBuffer.SetData(
						dataArray,
						i * blockSize,
						blockIndex * blockSize,
						instanceCount
					);
					chunkPositionBuffer.SetData(chunkPositionArray, 0, blockIndex, 1);
					newblockIndexList.Add(blockIndex);

					instanceCountArray[0] = instanceCount;
					instanceCountBuffer.SetData(instanceCountArray, 0, blockIndex, 1);

					typeIndexBuffer.SetData(chunkIndexArray, 0, blockIndex, 1);

					elementLeft -= blockSize;
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
				NativeArray<InstanceData> dataArray = data.AsArray();
				int elementLeft = dataArray.Length;

				// use existed block
				int[] instanceCountArray = new int[1];
				for (int i = 0; i < numBlockUsed; i++)
				{
					int instanceCount = math.min(elementLeft, blockSize);
					int blockIndex = blockIndexList[i];
					instanceBuffer.SetData(
						dataArray,
						i * blockSize,
						blockIndex * blockSize,
						instanceCount
					);
					chunkPositionBuffer.SetData(chunkPositionArray, 0, blockIndex, 1);

					instanceCountArray[0] = instanceCount;
					instanceCountBuffer.SetData(instanceCountArray, 0, blockIndex, 1);

					typeIndexBuffer.SetData(chunkIndexArray, 0, blockIndex, 1);

					elementLeft -= blockSize;
				}

				// use available block
				for (int i = 0; i < extraBlockNeeded; i++)
				{
					int instanceCount = math.min(elementLeft, blockSize);
					int blockIndex = availableBlockIndex.Dequeue();
					instanceBuffer.SetData(
						dataArray,
						i * blockSize,
						blockIndex * blockSize,
						math.min(elementLeft, blockSize)
					);
					chunkPositionBuffer.SetData(chunkPositionArray, 0, blockIndex, 1);
					blockIndexList.Add(blockIndex);

					// update instance number in the block
					instanceCountArray[0] = instanceCount;
					instanceCountBuffer.SetData(instanceCountArray, 0, blockIndex, 1);

					typeIndexBuffer.SetData(chunkIndexArray, 0, blockIndex, 1);

					elementLeft -= blockSize;
				}
			}
		}
	}
}