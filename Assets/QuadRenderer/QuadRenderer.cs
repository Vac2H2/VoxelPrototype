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

	Material InstanceShader;

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

		InitStructures();
	}

	public void Update()
	{
		Graphics.DrawMeshInstancedIndirect(
			dummyMesh,
			0,
			InstanceShader,
			bounds,
			argsBuffer,
			0,
			null,
			ShadowCastingMode.Off,
			true 
		);
	}
}