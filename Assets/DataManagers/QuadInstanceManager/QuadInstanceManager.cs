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

		int totalSize = maxNumBlock * BLOCK_SIZE;

		instanceBuffer = new ComputeBuffer(
			totalSize,
			sizeof(uint) * 2,
			ComputeBufferType.Structured
		);
		uint[] initInstanceArray = new uint[totalSize];
		instanceBuffer.SetData(initInstanceArray);

		chunkPositionBuffer = new ComputeBuffer(
			totalSize,
			sizeof(int) * 3,
			ComputeBufferType.Structured
		);
		int3[] initPositionArray = new int3[totalSize];
		chunkPositionBuffer.SetData(initPositionArray);

		instanceCountBuffer = new ComputeBuffer(
			totalSize,
			sizeof(int),
			ComputeBufferType.Structured
		);
		int[] initCountArray = new int[totalSize];
		instanceCountBuffer.SetData(initCountArray);

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