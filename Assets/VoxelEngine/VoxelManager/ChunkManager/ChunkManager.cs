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

        NativeHashMap<int3, int> chunkMap;
        int numChunkAdded;

        NativeHashSet<int3> dirtyChunks;
        TransformManager transformManager;

        public ChunkManager(int maxNumChunk)
        {
            numChunkAdded = 0;
            chunkMap = new NativeHashMap<int3, int>(maxNumChunk, Allocator.Persistent);
            dirtyChunks = new NativeHashSet<int3>(maxNumChunk, Allocator.Persistent);
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
    }
}
