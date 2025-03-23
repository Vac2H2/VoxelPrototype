using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

public abstract partial class VoxelDataManager
{
	public const int BLOCK_SIZE = 1024;
	public const int STATE_SIZE = 32 * 32;
	public const int TYPE_SIZE = 32 * 32 * 32;

	protected NativeHashMap<int3, int> chunkMap;
	protected NativeQueue<int3> dirtyChunks;

	protected NativeList<uint> stateList;
	protected NativeList<uint> typeList;

	protected ComputeBuffer typeBuffer;

	protected int numChunkAdded;
	protected Matrix4x4 localToWorld;
	protected Matrix4x4 worldToLocal;

	public VoxelDataManager(int maxNumChunk, Matrix4x4 _localToWorld)
	{
		localToWorld = _localToWorld;
		worldToLocal = _localToWorld.inverse;

		numChunkAdded = 0;
		chunkMap = new NativeHashMap<int3, int>(maxNumChunk, Allocator.Persistent);
		dirtyChunks = new NativeQueue<int3>(Allocator.Persistent);
		stateList = new NativeList<uint>(maxNumChunk, Allocator.Persistent);
		typeList = new NativeList<uint>(maxNumChunk, Allocator.Persistent);

		typeBuffer = new ComputeBuffer(
			maxNumChunk * VoxelDataManager.TYPE_SIZE,
			sizeof(uint),
			ComputeBufferType.Structured
		);
		int[] initTypeArray = new int[maxNumChunk * TYPE_SIZE];
		typeBuffer.SetData(initTypeArray);
	}

	public Matrix4x4 GetWorldToLocalMatrix()
	{
		return worldToLocal;
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

			numChunkAdded++;

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
		if (chunkMap.TryGetValue(chunkPosition, out int _))
		{
			dirtyChunks.Enqueue(chunkPosition);
		}
	}

	public NativeQueue<int3> GetDirtyChunkQueue()
	{
		return dirtyChunks;
	}

	public int GetChunkIndex(int3 chunkPosition)
	{
		if (chunkMap.TryGetValue(chunkPosition, out int chunkIndex))
		{
			return chunkIndex;
		}
		return -1;
	}

	public void UpdateTypeBuffer(int3 chunkPosition)
	{
		if (chunkMap.TryGetValue(chunkPosition, out int chunkIndex))
		{
			int startIndex = chunkIndex * TYPE_SIZE;
			NativeSlice<uint> slice = new NativeSlice<uint>(typeList.AsArray(), startIndex, TYPE_SIZE);
			typeBuffer.SetData(slice.ToArray(), 0, startIndex, TYPE_SIZE);
		}
	}

	public void DestroyBasicStructures()
	{
		chunkMap.Dispose();
		dirtyChunks.Dispose();
		stateList.Dispose();
		typeList.Dispose();
		typeBuffer.Release();
	}

	public ComputeBuffer GetTypeBuffer()
	{
		return typeBuffer;
	}
}