using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

public abstract partial class VoxelDataManager
{
	public const int BLOCK_SIZE = 1024;
	public const int STATE_SIZE = 32 * 32;
	public const int TYPE_SIZE = 32 * 32 * 32;

	protected NativeHashMap<int3, int> chunkMap;
	protected NativeHashMap<int, NativeList<int>> blockIndexMap;
	protected NativeQueue<int3> dirtyChunks;

	protected NativeList<uint> stateList;
	protected NativeList<uint> typeList;

	protected int numChunkAdded;

	public VoxelDataManager(int maxNumChunk)
	{
		chunkMap = new NativeHashMap<int3, int>(maxNumChunk, Allocator.Persistent);
		blockIndexMap = new NativeHashMap<int, NativeList<int>>(maxNumChunk, Allocator.Persistent);
		dirtyChunks = new NativeQueue<int3>(Allocator.Persistent);
		stateList = new NativeList<uint>(maxNumChunk, Allocator.Persistent);
		typeList = new NativeList<uint>(maxNumChunk, Allocator.Persistent);
	}

	/// <summary>
	/// Add a chunk to data structure
	/// </summary>
	/// <param name="chunkPosition">The position you want to create a new chunk</param>
	/// <returns>Is chunk successfully added? True -> added, False -> already existed</returns>
	public bool AddChunk(int3 chunkPosition)
	{
		if (chunkMap.TryAdd(chunkPosition, numChunkAdded))
		{
			NativeArray<uint> state = new NativeArray<uint>(STATE_SIZE, Allocator.Temp, NativeArrayOptions.ClearMemory);
			NativeArray<uint> type = new NativeArray<uint>(TYPE_SIZE, Allocator.Temp, NativeArrayOptions.ClearMemory);

			stateList.AddRange(state);
			typeList.AddRange(type);

			return true;
		}
		return false;
	}

	/// <summary>
	/// Retrieves a pair of NativeSlice views (state and type) for the given chunk position.
	/// </summary>
	/// <param name="chunkPosition">The position of the chunk to retrieve slices for.</param>
	/// <returns>
	/// A tuple where:
	///   - <c>found</c> indicates if the chunk was located,
	///   - <c>state</c> is the corresponding state slice,
	///   - <c>type</c> is the corresponding type slice.
	/// If the chunk is not found, <c>found</c> is false and the slices are set to default.
	/// </returns>
	public (bool found, NativeSlice<uint> state, NativeSlice<uint> type) GetChunkSlice(int3 chunkPosition)
	{
		if (chunkMap.TryGetValue(chunkPosition, out int chunkIndex))
		{
			NativeSlice<uint> stateSlice = new NativeSlice<uint>(
				stateList.AsArray(),
				chunkIndex * STATE_SIZE,
				STATE_SIZE
			);
			NativeSlice<uint> typeSlice = new NativeSlice<uint>(
				typeList.AsArray(),
				chunkIndex * TYPE_SIZE,
				TYPE_SIZE
			);
			return (true, stateSlice, typeSlice);
		}
		return (false, default, default);
	}

	/// <summary>
	/// Make a chunk dirty in given position. Not check chunk existance.
	/// </summary>
	/// <param name="chunkPosition">The chunk position</param>
	public void AddDirtyFlag(int3 chunkPosition)
	{
		dirtyChunks.Enqueue(chunkPosition);
	}

	public void DestroyBasicStructures()
	{
		chunkMap.Dispose();
		blockIndexMap.Dispose();
		dirtyChunks.Dispose();
		stateList.Dispose();
		typeList.Dispose();
	}
}