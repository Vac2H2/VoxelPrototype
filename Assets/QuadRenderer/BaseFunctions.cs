using UnityEngine;

public partial class QuadRenderer
{
	void InitDummyMesh()
	{
		dummyMesh = new Mesh();
		Vector3[] vertices = new Vector3[4]
		{
			new Vector3(0, 0, 0),  // Vertex 0
            new Vector3(0, 1, 0),  // Vertex 1
            new Vector3(1, 1, 0),  // Vertex 2
            new Vector3(1, 0, 0)   // Vertex 3
        };
		int[] triangles = new int[6] { 0, 1, 2, 0, 2, 3 };
		dummyMesh.vertices = vertices;
		dummyMesh.triangles = triangles;
	}

	void InitBounds()
	{
		bounds = new Bounds(Vector3.zero, new Vector3(1000, 1000, 1000));
	}

	void InitArgsBuffer()
	{
		// args
		uint[] args = new uint[5];
		args[0] = dummyMesh.GetIndexCount(0);  // 6
		args[1] = 0;
		args[2] = dummyMesh.GetIndexStart(0);  // 0
		args[3] = dummyMesh.GetBaseVertex(0);  // 0
		args[4] = 0;
		argsBuffer = new ComputeBuffer(
			1,
			args.Length * sizeof(uint),
			ComputeBufferType.IndirectArguments
		);
		argsBuffer.SetData(args);
	}

	void InitShader()
	{
		InstanceShader = new Material(Shader.Find("Shader Graphs/InstanceShader"));
		InstanceShader.SetBuffer("_instanceBuffer", instanceBuffer);
		InstanceShader.SetBuffer("_instanceCountBuffer", instanceCountBuffer);
		InstanceShader.SetBuffer("_chunkPositionBuffer", chunkPositionBuffer);
		InstanceShader.SetBuffer("_chunkIndexBuffer", typeIndexBuffer);
		InstanceShader.SetBuffer("_typeBuffer", typeBuffer);
	}

	void InitStructures()
	{
		InitBounds();
		InitDummyMesh();
		InitArgsBuffer();
		InitShader();
		UpdateArgsBuffer((uint)instanceBuffer.count);
	}

	void UpdateArgsBuffer(uint numInstance)
	{
		uint[] args = new uint[5];
		args[0] = dummyMesh.GetIndexCount(0);  // 6
		args[1] = numInstance;
		args[2] = dummyMesh.GetIndexStart(0);  // 0
		args[3] = dummyMesh.GetBaseVertex(0);  // 0
		args[4] = 0;
		argsBuffer.SetData(args);
	}

	public void Destroy()
	{
		argsBuffer.Release();
	}
}