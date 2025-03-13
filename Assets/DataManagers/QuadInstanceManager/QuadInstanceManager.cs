using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

public abstract partial class QuadInstanceManager
{
	public const int BLOCK_SIZE = 1024;

	protected NativeHashMap<int3, NativeList<int>> chunkToBlocksMap;
	protected NativeQueue<int> availableBlockIndex;

	protected ComputeBuffer instanceBuffer;
	protected ComputeBuffer chunkPositionBuffer;
	protected ComputeBuffer instanceCountBuffer;

	public QuadInstanceManager(int maxNumChunk, int maxNumBlock)
	{
		chunkToBlocksMap = new NativeHashMap<int3, NativeList<int>>(maxNumChunk, Allocator.Persistent);
		availableBlockIndex = new NativeQueue<int>(Allocator.Persistent);

		instanceBuffer = new ComputeBuffer(
			maxNumBlock * BLOCK_SIZE,
			sizeof(uint),
			ComputeBufferType.Structured
		);
		chunkPositionBuffer = new ComputeBuffer(
			maxNumBlock * BLOCK_SIZE,
			sizeof(int) * 3,
			ComputeBufferType.Structured
		);
		instanceCountBuffer = new ComputeBuffer(
			maxNumBlock * BLOCK_SIZE,
			sizeof(int),
			ComputeBufferType.Structured
		);

		for (int i = 0; i < maxNumBlock; i++)
		{
			availableBlockIndex.Enqueue(i);
		}
	}

	/// <summary>
	/// Add a chunk to data structure. Create new GPU block index mapping list for this chunk.
	/// </summary>
	/// <param name="chunkPosition">The position you want to create a new chunk</param>
	/// <returns>Is chunk successfully added? True -> added, False -> already existed</returns>
	public bool AddChunk(int3 chunkPosition)
	{
		NativeList<int> blockIndices = new NativeList<int>(4, Allocator.Persistent);
		if (chunkToBlocksMap.TryAdd(chunkPosition, blockIndices))
		{
			return true;
		}
		return false;
	}

	public void DestroyBasicStructures()
	{
		chunkToBlocksMap.Dispose();
		availableBlockIndex.Dispose();
		instanceBuffer.Release();
		chunkPositionBuffer.Release();
		instanceCountBuffer.Release();
	}
}