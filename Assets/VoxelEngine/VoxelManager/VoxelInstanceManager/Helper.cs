using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Jobs;

namespace VoxelEngine.VoxelManager
{
	public partial class VoxelInstanceManager
	{
		/// <summary>
		/// A job to extract all block indices from the 'visible' chunks.
		/// The final data is appended into AllBlockIndices.
		/// </summary>
		public struct GatherBlocksJob : IJobParallelFor
		{
			[ReadOnly] public NativeList<int3> VisibleChunks;
			[ReadOnly] public NativeHashMap<int3, NativeList<int>> ChunkToBlocksMap;

			// A parallel-writer to our big NativeList of blocks
			public NativeList<int>.ParallelWriter AllBlocksWriter;

			public void Execute(int index)
			{
				int3 chunkPos = VisibleChunks[index];
				if (ChunkToBlocksMap.TryGetValue(chunkPos, out NativeList<int> blockList))
				{
					for (int j = 0; j < blockList.Length; j++)
					{
						// Each call does an atomic "reserve index" in the underlying NativeList:
						AllBlocksWriter.AddNoResize(blockList[j]);
					}
				}
			}
		}

		NativeList<int> GetVisibleGPUBlocks(NativeList<int3> visibleChunks)
		{
			int numVisible = visibleChunks.Length;
			if (numVisible == 0)
			{
				return new NativeList<int>(0, Allocator.Persistent);
			}

			int capacityGuess = maxNumChunk * maxNumBlockPerChunk;
			NativeList<int> allBlocks = new NativeList<int>(capacityGuess, Allocator.Persistent);

			// 2) Prepare a parallel writer
			var allBlocksWriter = allBlocks.AsParallelWriter();

			// 3) Schedule the gather job
			var gatherJob = new GatherBlocksJob
			{
				VisibleChunks     = visibleChunks,
				ChunkToBlocksMap  = chunkToBlocksMap,
				AllBlocksWriter   = allBlocksWriter
			};

			JobHandle handle = gatherJob.Schedule(numVisible, 64);
			handle.Complete();

			return allBlocks;
		}
	}
}