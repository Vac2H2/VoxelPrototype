using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

namespace VoxelEngine.VoxelManager
{
    /// <summary>
    /// Chunk Manager stores mapping of chunk position to index.
    /// It stores dirty state of chunks.
    /// It stores transformation data.
    /// </summary>
    public partial class ChunkManager
    {
        public const int CHUNK_WIDTH = 32;

        int numChunkAdded;
        NativeHashMap<int3, int> chunkMap;
        NativeHashSet<int3> dirtyChunks;

        TransformManager transformManager;
        VoxelStateManager voxelStateManager;
        VoxelTypeManager voxelTypeManager;
        VoxelInstanceManager voxelInstanceManager;

        public ChunkManager(int maxNumChunk, int maxNumBlockPerChunk, int gpuBlockSize, float3 scale)
        {
            numChunkAdded = 0;
            chunkMap = new NativeHashMap<int3, int>(maxNumChunk, Allocator.Persistent);
            dirtyChunks = new NativeHashSet<int3>(maxNumChunk, Allocator.Persistent);

            transformManager = new TransformManager(maxNumChunk, scale);
            voxelStateManager = new VoxelStateManager(maxNumChunk);
            voxelTypeManager = new VoxelTypeManager(maxNumChunk);
            voxelInstanceManager = new VoxelInstanceManager(maxNumChunk, maxNumBlockPerChunk, gpuBlockSize);
        }

        public void AddChunk(int3 chunkPosition)
        {
            if (chunkMap.TryAdd(chunkPosition, numChunkAdded))
            {
                transformManager.AddChunk(chunkPosition);
                SetChunkDirty(chunkPosition);
                numChunkAdded++;
            }
        }

        public void SetChunkDirty(int3 chunkPosition)
        {
            if (chunkMap.ContainsKey(chunkPosition))
            {
                dirtyChunks.Add(chunkPosition);
            }
        }

        public
        (
            float3 scale,
            ComputeBuffer instanceBuffer,
            ComputeBuffer instanceCountBuffer,
            ComputeBuffer chunkPositionBuffer,
            ComputeBuffer typeIndexBuffer,
            ComputeBuffer typeBuffer,
            ComputeBuffer visibleBlockBuffer,
            ComputeBuffer visibleBlockCountBuffer
        ) GetRendererData()
        {
            (
                ComputeBuffer instanceBuffer,
                ComputeBuffer instanceCountBuffer,
                ComputeBuffer chunkPositionBuffer,
                ComputeBuffer typeIndexBuffer,
                ComputeBuffer visibleBlockBuffer,
                ComputeBuffer visibleBlockCountBuffer
            ) = voxelInstanceManager.GetBuffers();

            float3 scale = transformManager.GetScale();

            ComputeBuffer typeBuffer = voxelTypeManager.GetTypeBuffer();

            return
            (
                scale,
                instanceBuffer,
                instanceCountBuffer,
                chunkPositionBuffer,
                typeIndexBuffer,
                typeBuffer,
                visibleBlockBuffer,
                visibleBlockCountBuffer
            );
        }

        public void Dispose()
        {
            chunkMap.Dispose();
            dirtyChunks.Dispose();
            transformManager.Dispose();
            voxelStateManager.Dispose();
            voxelInstanceManager.Dispose();
        }
    }
}
