using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Burst;

namespace VoxelEngine.VoxelManager
{
	public partial class TransformManager
	{
		public static float4 ToFloat4(Plane plane)
		{
			return new float4(plane.normal.x, plane.normal.y, plane.normal.z, plane.distance);
		}

		public static void GetFrustumPlanes(Camera cam, NativeArray<float4> planesOut)
		{
			Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);

			for (int i = 0; i < 6; i++)
			{
				planesOut[i] = ToFloat4(planes[i]);
			}
		}

		[BurstCompile]
		public struct FrustumCullingJob : IJobFor
		{
			[ReadOnly] public NativeArray<float4> FrustumPlanes;     // 6 planes
			[ReadOnly] public NativeArray<Bounds> ChunkBounds;       // same length as chunkPositions
			[ReadOnly] public NativeArray<int3> ChunkPositions;   // same length as chunkBounds

			// ParallelWriter lets multiple threads safely add to this list
			public NativeList<int3>.ParallelWriter VisibleChunks;

			public void Execute(int index)
			{
				var bounds = ChunkBounds[index];
				if (IsInFrustum(bounds, FrustumPlanes))
				{
					VisibleChunks.AddNoResize(ChunkPositions[index]);
				}
			}

			private bool IsInFrustum(Bounds bounds, NativeArray<float4> planes)
			{
				// Basic bounding-sphere test from an AABB:
				float3 center = (float3)bounds.center;
				float3 extents = (float3)bounds.extents;

				// Check each plane
				for (int i = 0; i < planes.Length; i++)
				{
					float4 p = planes[i];
					float3 normal = p.xyz;
					float dist = p.w;

					// Compute the “radius” of bounds relative to plane normal.
					float r = extents.x * math.abs(normal.x)
							+ extents.y * math.abs(normal.y)
							+ extents.z * math.abs(normal.z);

					// Distance from center to plane
					float d = math.dot(normal, center) + dist;

					// If entire bounding volume is behind the plane, cull it.
					if (d < -r)
					{
						return false;
					}
				}

				return true; // inside all planes
			}
		}
	}
}