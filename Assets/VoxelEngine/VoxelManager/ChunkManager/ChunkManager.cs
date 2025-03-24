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

        public ChunkManager(int maxNumChunk, float3 scale)
        {
            numChunkAdded = 0;
            chunkMap = new NativeHashMap<int3, int>(maxNumChunk, Allocator.Persistent);
            dirtyChunks = new NativeHashSet<int3>(maxNumChunk, Allocator.Persistent);

            transformManager = new TransformManager(maxNumChunk, scale);
            voxelStateManager = new VoxelStateManager(maxNumChunk);
        }

        public void AddChunk(int3 chunkPosition)
        {
            if (chunkMap.TryAdd(chunkPosition, numChunkAdded))
            {
                transformManager.AddChunk(chunkPosition, numChunkAdded);
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
    }
}
