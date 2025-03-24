using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

namespace VoxelEngine.VoxelManager
{
	/// <summary>
	/// Transform Manager stores localScale and position of a voxel space.
	/// It stores bounding boxes of chunks.
	/// It stores frustum culling data of chunks.
	/// </summary>
	public partial class TransformManager
	{
		float3 scale;
		NativeList<int3> chunkAdded;
		NativeList<Bounds> chunkBounds;

		public TransformManager(int maxNumChunk, float3 _scale)
		{
			scale = _scale;

			chunkAdded = new NativeList<int3>(maxNumChunk, Allocator.Persistent);
			chunkBounds = new NativeList<Bounds>(maxNumChunk, Allocator.Persistent);
		}

		public void AddChunk(int3 chunkPosition)
		{
			chunkAdded.Add(chunkPosition);
			AddChunkBounds(chunkPosition);
		}

		public void AddChunkBounds(int3 chunkPosition)
		{
			int3 actualPosition = chunkPosition * ChunkManager.CHUNK_WIDTH;
			float3 chunkCenter = ((float3)actualPosition + ChunkManager.CHUNK_WIDTH * 0.5f) * scale;
			float3 chunkScales = new float3(
				ChunkManager.CHUNK_WIDTH,
				ChunkManager.CHUNK_WIDTH,
				ChunkManager.CHUNK_WIDTH
			) * scale;
			chunkBounds.Add(new Bounds(chunkCenter, chunkScales));
		}
    }
}