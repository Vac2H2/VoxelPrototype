using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace VoxelEngine.VoxelRenderer
{
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

		void InitPreprocessShader()
		{
			preprocessShader = Resources.Load<ComputeShader>("PreprocessShader");
			int kernel = preprocessShader.FindKernel("PreprocessInstance");

			preprocessShader.SetMatrix("_TransformationMatrix", transformationMatrix);
			preprocessShader.SetBuffer(kernel, "_instanceBuffer", instanceBuffer);
			preprocessShader.SetBuffer(kernel, "_instanceCountBuffer", instanceCountBuffer);
			preprocessShader.SetBuffer(kernel, "_chunkPositionBuffer", chunkPositionBuffer);
			preprocessShader.SetBuffer(kernel, "_typeIndexBuffer", typeIndexBuffer);

			validInstanceBuffer = new ComputeBuffer(
				instanceBuffer.count,
				44,
				ComputeBufferType.Append
			);
			preprocessShader.SetBuffer(kernel, "_validInstanceBuffer", validInstanceBuffer);
		}

		void InitShader()
		{
			InstanceShader = new Material(Shader.Find("Shader Graphs/InstanceShader"));
			InstanceShader.SetBuffer("_validInstanceBuffer", validInstanceBuffer);
			InstanceShader.SetBuffer("_typeBuffer", typeBuffer);
			InstanceShader.SetMatrix("_TransformationMatrix", transformationMatrix);
		}

		void InitStructures()
		{
			InitBounds();
			InitDummyMesh();
			InitArgsBuffer();
			UpdateArgsBuffer((uint)instanceBuffer.count);

			InitPreprocessShader();
			InitShader();
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
			validInstanceBuffer.Release();
		}

		public void PreprocessInstance()
		{
			int numGroup = (instanceBufferCount + WORKGROUP_SIZE - 1) / WORKGROUP_SIZE;
			int kernel = preprocessShader.FindKernel("PreprocessInstance");
			preprocessShader.SetVector("_cameraPosition", Camera.main.transform.position);
			preprocessShader.Dispatch(kernel, numGroup, 1, 1);

			// offset of 4 bytes for args[1]
			ComputeBuffer.CopyCount(validInstanceBuffer, argsBuffer, 4);
		}

		public void IssueRenderCycle()
		{
			PreprocessInstance();
			
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
			
			validInstanceBuffer.SetCounterValue(0);
		}
	}
}