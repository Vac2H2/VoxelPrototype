using UnityEngine;
using UnityEngine.Rendering;

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

	int instanceBufferCount;

	public QuadRenderer(
		ComputeBuffer _instanceBuffer,
		ComputeBuffer _instanceCountBuffer,
		ComputeBuffer _chunkPositionBuffer,
		ComputeBuffer _typeIndexBuffer,
		ComputeBuffer _typeBuffer
	)
	{
		instanceBuffer = _instanceBuffer;
		instanceCountBuffer = _instanceCountBuffer;
		chunkPositionBuffer = _chunkPositionBuffer;
		typeIndexBuffer = _typeIndexBuffer;
		typeBuffer = _typeBuffer;

		instanceBufferCount = instanceBuffer.count;

		InitStructures();
	}

	public void Update()
	{
		IssueRenderCycle();
	}
}