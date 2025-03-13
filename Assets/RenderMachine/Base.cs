// using Unity.Mathematics;
// using UnityEngine;
// using UnityEngine.Rendering;

// public partial class RenderMachine : MonoBehaviour
// {
//     Material mat;
//     Mesh dummyMesh;
//     ComputeBuffer argsBuffer;
//     SpaceDataManagerOptimized dataManager;
//     Bounds bounds;

//     void GetSpaceDataManager()
//     {
//         dataManager = GetComponentInParent<SpaceDataManager>().GetSpaceDataManager();
//     }

//     void InitShader()
//     {
//         SpaceDataManagerOptimized.PackedData data = dataManager.GetPackedData();

//         mat = new Material(Shader.Find("Shader Graphs/InstancingShaderLit"));
//         Debug.Log("initShader");
//         Debug.Log(mat);
//         mat.SetBuffer("_instanceBuffer", data.InstanceBuffer);
//         mat.SetBuffer("_chunkIndexBuffer", data.ChunkIndexBuffer);
//         mat.SetBuffer("_instanceCountBuffer", data.InstanceCountBuffer);
//         mat.SetBuffer("_chunkPositionBuffer", data.ChunkPositionBuffer);
//         mat.SetBuffer("_typeBuffer", data.TypeBuffer);
//     }

//     void InitDummyMesh()
//     {
//         // dummy mesh
//         dummyMesh = new Mesh();
//         Vector3[] vertices = new Vector3[4]
//         {
//             new Vector3(0, 0, 0),  // Vertex 0
//             new Vector3(0, 1, 0),  // Vertex 1
//             new Vector3(1, 1, 0),  // Vertex 2
//             new Vector3(1, 0, 0)   // Vertex 3
//         };
//         int[] triangles = new int[6] { 0, 1, 2, 0, 2, 3 };
//         dummyMesh.vertices = vertices;
//         dummyMesh.triangles = triangles;
//     }

//     void InitBounds()
//     {
//         bounds = new Bounds(Vector3.zero, new Vector3(1000, 1000, 1000));
//     }

//     void InitArgsBuffer()
//     {
//         // args
//         uint[] args = new uint[5];
//         args[0] = dummyMesh.GetIndexCount(0);  // 6
//         args[1] = 0;
//         args[2] = dummyMesh.GetIndexStart(0);  // 0
//         args[3] = dummyMesh.GetBaseVertex(0);  // 0
//         args[4] = 0;
//         argsBuffer = new ComputeBuffer(
//             1,
//             args.Length * sizeof(uint),
//             ComputeBufferType.IndirectArguments
//         );
//         argsBuffer.SetData(args);
//     }

//     public void DrawInstances()
//     {
//         uint currentNumInstance = (uint)dataManager.GetInstanceBufferCount();
//         if (previousNumInstance != currentNumInstance)
//         {
//             uint[] args = new uint[5];
//             args[0] = dummyMesh.GetIndexCount(0);  // 6
//             args[1] = currentNumInstance;
//             args[2] = dummyMesh.GetIndexStart(0);  // 0
//             args[3] = dummyMesh.GetBaseVertex(0);  // 0
//             args[4] = 0;
//             argsBuffer.SetData(args);
//             previousNumInstance = currentNumInstance;
//         }

//         // update rotation matrix
//         Matrix4x4 currentTransformationMatrix = dataManager.GetTransformationMatrix();
//         if (currentTransformationMatrix != transformationMatrix)
//         {
//             transformationMatrix = currentTransformationMatrix;
//             mat.SetMatrix("_TransformationMatrix", currentTransformationMatrix);
//         }

//         Graphics.DrawMeshInstancedIndirect(
//             dummyMesh,
//             0,
//             mat,
//             bounds,
//             argsBuffer,
//             0,
//             null,
//             ShadowCastingMode.On,
//             true 
//         );
//     }

//     void OnDestroy()
//     {
//         argsBuffer.Release();
//     }
// }
