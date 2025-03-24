using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

namespace VoxelEngine.VoxelManager.ChunkManager
{
    /// <summary>
    /// Chunk Manager stores mapping of chunk position to index.
    /// It stores dirty state of chunks.
    /// </summary>
    public partial class ChunkManager
    {
        NativeHashMap<int3, int> chunkMap;
        NativeHashSet<int3> dirtyChunks;

        public ChunkManager(int maxNumChunk)
        {
            chunkMap = new NativeHashMap<int3, int>(maxNumChunk, Allocator.Persistent);
            dirtyChunks = new NativeHashSet<int3>(maxNumChunk, Allocator.Persistent);
        }
    }
}
