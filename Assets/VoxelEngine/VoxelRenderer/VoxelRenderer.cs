using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace VoxelEngine.VoxelRendering
{
	public partial class VoxelRenderer
	{
		Mesh dummyMesh;
		Bounds bounds;
		float3 scale;
		ComputeBuffer argsBuffer;
		ComputeBuffer instanceBuffer;
		ComputeBuffer instanceCountBuffer;
		ComputeBuffer chunkPositionBuffer;
		ComputeBuffer typeIndexBuffer;
		ComputeBuffer typeBuffer;
		ComputeBuffer visibleBlockBuffer;
		ComputeBuffer visibleBlockCountBuffer;

		ComputeShader preprocessShader;
		public const int WORKGROUP_SIZE = 64;
		ComputeBuffer validInstanceBuffer;
		Material InstanceShader;

		Matrix4x4 transformationMatrix;

		int instanceBufferCount;

		public VoxelRenderer(
			float3 _scale,
			ComputeBuffer _instanceBuffer,
			ComputeBuffer _instanceCountBuffer,
			ComputeBuffer _chunkPositionBuffer,
			ComputeBuffer _typeIndexBuffer,
			ComputeBuffer _typeBuffer,
			ComputeBuffer _visibleBlockBuffer,
			ComputeBuffer _visibleBlockCountBuffer
		)
		{
			scale = _scale;
			instanceBuffer = _instanceBuffer;
			instanceBufferCount = instanceBuffer.count;
			instanceCountBuffer = _instanceCountBuffer;
			chunkPositionBuffer = _chunkPositionBuffer;
			typeIndexBuffer = _typeIndexBuffer;
			typeBuffer = _typeBuffer;

			visibleBlockBuffer = _visibleBlockBuffer;
			visibleBlockCountBuffer = _visibleBlockCountBuffer;

			InitStructures();
		}

		public void Update()
		{
			IssueRenderCycle();
		}
	}
}