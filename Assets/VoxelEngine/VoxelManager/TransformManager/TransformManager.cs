using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

namespace VoxelEngine.VoxelManager.TransformManager
{
    /// <summary>
    /// Transform Manager stores localScale and position of a voxel space.
    /// It stores bounding boxes of chunks.
	/// It stores frustum culling data of chunks.
    /// </summary>
    public partial class TransformManager
    {
		float3 scale;
		NativeArray<int3> chunkAdded;
		NativeArray<Bounds> chunkBounds;
		NativeHashMap<int3, int> chunkMap;

		public TransformManager(int maxNumChunk, float3 _scale, NativeHashMap<int3, int> _chunkMap)
		{
			scale = _scale;
			chunkMap = _chunkMap;

			chunkAdded = new NativeArray<int3>(maxNumChunk, Allocator.Persistent);
			chunkBounds = new NativeArray<Bounds>(maxNumChunk, Allocator.Persistent);
		}
    }
}