using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Burst;

namespace VoxelEngine.VoxelManager
{
	/// <summary>
	/// Transform Manager stores localScale and position of a voxel space.
	/// It stores bounding boxes of chunks.
	/// It stores frustum culling data of chunks.
	/// </summary>
	public partial class TransformManager
	{
		int maxNumChunk;
		float3 scale;
		NativeList<int3> chunkPositions;
		NativeList<int> chunkIndices;
		NativeList<Bounds> chunkBounds;

		public TransformManager(int _maxNumChunk, float3 _scale)
		{
			maxNumChunk = _maxNumChunk;
			scale = _scale;

			chunkPositions = new NativeList<int3>(maxNumChunk, Allocator.Persistent);
			chunkBounds = new NativeList<Bounds>(maxNumChunk, Allocator.Persistent);
		}

		public void AddChunk(int3 chunkPosition)
		{
			chunkPositions.Add(chunkPosition);
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

		public float3 GetScale()
		{
			return scale;
		}

		public NativeList<int3> GetVisibleChunkPositions()
		{
			// 1) Allocate plane array and fill it on main thread
			NativeArray<float4> frustumPlanes = new NativeArray<float4>(6, Allocator.TempJob);
			GetFrustumPlanes(Camera.main, frustumPlanes);

			// 2) Prepare our output list + parallel writer
			var visibleChunks = new NativeList<int3>(maxNumChunk, Allocator.TempJob);
			var visibleChunksWriter = visibleChunks.AsParallelWriter();

			// 3) Create and schedule the job
			var job = new FrustumCullingJob
			{
				FrustumPlanes = frustumPlanes,
				ChunkBounds = chunkBounds.AsArray(),
				ChunkPositions = chunkPositions.AsArray(),
				VisibleChunks = visibleChunksWriter
			};

			// IJobFor -> you schedule with job.Schedule(chunkBounds.Length, batchSize) in older versions
			// If using 2022+ you do job.ScheduleParallel.
			// For older Unity: job.Schedule(chunkBounds.Length, 64).Complete();
			job.ScheduleParallel(chunkBounds.Length, 64, default).Complete();

			// 4) Dispose of planes array
			frustumPlanes.Dispose();

			// Now visibleChunks contains all chunks that passed the frustum test
			return visibleChunks;
		}

		public void Dispose()
		{
			chunkPositions.Dispose();
			chunkIndices.Dispose();
			chunkBounds.Dispose();
		}
    }
}