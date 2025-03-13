using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using System;

public partial class SpaceDataManagerOptimized
{
    public const int BLOCK_SIZE = 1024;
    public const int STATE_SIZE = 32 * 32;
    public const int TYPE_SIZE = 32 * 32 * 32;

    Matrix4x4 transformationMatrix = Matrix4x4.identity;

    int numChunkAdded;
    NativeHashMap<int3, int> chunkMap;
    NativeHashMap<int, NativeList<int>> blockIndexMap;

    NativeQueue<int> availableBlockIndex;

    ComputeBuffer instanceBuffer;
    ComputeBuffer chunkIndexBuffer;
    ComputeBuffer chunkPositionBuffer;
    ComputeBuffer instanceCountBuffer;

    NativeList<uint> stateList;
    NativeList<uint> typeList;
    ComputeBuffer typeBuffer;

    NativeQueue<int3> dirtyChunks;


    ComputeShader copyShader = Resources.Load<ComputeShader>("QuadInstancing/UpdateInstanceDataOnGPU");

    public SpaceDataManagerOptimized(int maximumNumChunks, int initialNumBlock)
    {
        numChunkAdded = 0;

        chunkMap = new NativeHashMap<int3, int>(maximumNumChunks, Allocator.Persistent);
        // record id of blocks used for a chunk
        blockIndexMap = new NativeHashMap<int, NativeList<int>>(maximumNumChunks, Allocator.Persistent);

        availableBlockIndex = new NativeQueue<int>(Allocator.Persistent);

        instanceBuffer = new ComputeBuffer(initialNumBlock * BLOCK_SIZE, sizeof(uint) * 2, ComputeBufferType.Structured);
        // record the actual number of instance in each block
        instanceCountBuffer
        = new ComputeBuffer(initialNumBlock * BLOCK_SIZE, sizeof(int), ComputeBufferType.Structured);

        // each block has chunkIndex -> chunkPosition | type
        chunkIndexBuffer = new ComputeBuffer(initialNumBlock, sizeof(int), ComputeBufferType.Structured);
        chunkPositionBuffer = new ComputeBuffer(initialNumBlock, sizeof(int) * 3, ComputeBufferType.Structured);

        stateList = new NativeList<uint>(initialNumBlock * STATE_SIZE, Allocator.Persistent);
        typeList = new NativeList<uint>(initialNumBlock * TYPE_SIZE, Allocator.Persistent);

        typeBuffer = new ComputeBuffer(initialNumBlock * TYPE_SIZE, sizeof(uint), ComputeBufferType.Structured);

        for (int i = 0; i < initialNumBlock; i++)
        {
            availableBlockIndex.Enqueue(i);
        }

        dirtyChunks = new NativeQueue<int3>(Allocator.Persistent);
    }

    public void Destroy()
    {
        chunkMap.Dispose();
        using (NativeArray<int> keys = blockIndexMap.GetKeyArray(Allocator.Temp))
        {
            for (int i = 0; i < keys.Length; i++)
            {
                int key = keys[i];
                blockIndexMap[key].Dispose();
            }
        }
        blockIndexMap.Dispose();
        availableBlockIndex.Dispose();
        instanceBuffer.Release();
        chunkIndexBuffer.Release();
        chunkPositionBuffer.Release();
        stateList.Dispose();
        typeList.Dispose();
        typeBuffer.Release();
        instanceCountBuffer.Release();
        dirtyChunks.Dispose();
    }
}
