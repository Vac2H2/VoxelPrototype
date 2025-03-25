using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace VoxelEngine.VoxelRenderer
{
	public partial class QuadRenderer
	{
		Mesh dummyMesh;
		Bounds bounds;
		ComputeBuffer argsBuffer;
		ComputeBuffer instanceBuffer;
		ComputeBuffer instanceCountBuffer;
		ComputeBuffer chunkPositionBuffer;
		ComputeBuffer typeIndexBuffer;
		ComputeBuffer typeBuffer;

		ComputeShader preprocessShader;
		public const int WORKGROUP_SIZE = 64;
		ComputeBuffer validInstanceBuffer;
		Material InstanceShader;

		Matrix4x4 transformationMatrix;

		int instanceBufferCount;

		public QuadRenderer(
			ComputeBuffer _instanceBuffer,
			ComputeBuffer _instanceCountBuffer,
			ComputeBuffer _chunkPositionBuffer,
			ComputeBuffer _typeIndexBuffer,
			ComputeBuffer _typeBuffer,
			Matrix4x4 _transformationMatrix
		)
		{
			instanceBuffer = _instanceBuffer;
			instanceCountBuffer = _instanceCountBuffer;
			chunkPositionBuffer = _chunkPositionBuffer;
			typeIndexBuffer = _typeIndexBuffer;
			typeBuffer = _typeBuffer;
			transformationMatrix = _transformationMatrix;

			instanceBufferCount = instanceBuffer.count;

			InitStructures();
		}

		public void Update()
		{
			IssueRenderCycle();
		}
	}
}