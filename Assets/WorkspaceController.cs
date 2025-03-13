// using UnityEngine;
// using Unity.Mathematics;
// using Unity.Collections;

// public class WorkspaceController : MonoBehaviour, SpaceDataManager
// {
//     public GameObject controlCube;
//     Matrix4x4 transformationMatrix;

//     SpaceDataManagerOptimized spaceDataManager;
//     // float timer = 0f;
//     void Start()
//     {
//         spaceDataManager = new SpaceDataManagerOptimized(10000, 100);
//         NativeArray<uint> state = new NativeArray<uint>(32 * 32, Allocator.Persistent);
//         NativeArray<uint> type = new NativeArray<uint>(32 * 32 * 32, Allocator.Persistent);

//         // all stone
//         for (int x = 0; x < 32; x++)
//         {
//             for (int y = 0; y < 32; y++)
//             {
//                 for (int z = 0; z < 32; z++)
//                 {
//                     type[x + 32 * y + 32 * 32 * z] = 7;
//                 }
//             }
//         }

//         // upper layer snow
//         for (int x = 0; x < 32; x++)
//         {
//             for (int y = 27; y < 32; y++)
//             {
//                 for (int z = 0; z < 32; z++)
//                 {
//                     type[x + 32 * y + 32 * 32 * z] = 6;
//                 }
//             }
//         }

//         for (int i = 0; i < state.Length; i++)
//         {
//             state[i] = uint.MaxValue;
//         }

//         spaceDataManager.AddChunk(int3.zero, state, type);
//         spaceDataManager.AddChunk(new int3(1, 0, 0), state, type);


//         state.Dispose();
//         type.Dispose();
//     }

//     void Update()
//     {
//         // timer += Time.deltaTime;
//         // if (timer >= 0.1f)
//         // {
//         //     NativeArray<uint> state = new NativeArray<uint>(32 * 32, Allocator.Persistent);
//         //     NativeArray<uint> type = new NativeArray<uint>(32 * 32 * 32, Allocator.Persistent);
//         //     VoxelQuadsGeneration.AddVoxel(state, UnityEngine.Random.Range(0, 5), UnityEngine.Random.Range(0, 5), UnityEngine.Random.Range(0, 5));
//         //     VoxelQuadsGeneration.AddVoxel(state, UnityEngine.Random.Range(0, 5), UnityEngine.Random.Range(0, 5), UnityEngine.Random.Range(0, 5));
//         //     VoxelQuadsGeneration.AddVoxel(state, UnityEngine.Random.Range(0, 5), UnityEngine.Random.Range(0, 5), UnityEngine.Random.Range(0, 5));
//         //     VoxelQuadsGeneration.AddVoxel(state, UnityEngine.Random.Range(0, 5), UnityEngine.Random.Range(0, 5), UnityEngine.Random.Range(0, 5));

//         //     NativeSlice<uint> stateSlice;
//         //     spaceDataManager.GetChunkStateSlice(int3.zero, out stateSlice);

//         //     stateSlice.CopyFrom(state);
//         //     spaceDataManager.SetChunkDirty(int3.zero);

//         //     timer = 0f;
//         // }
//         UpdateTransformationMatrix();
//     }

//     public SpaceDataManagerOptimized GetSpaceDataManager()
//     {
//         return spaceDataManager;
//     }

//     void OnDestroy()
//     {
//         spaceDataManager.Destroy();
//     }

//     public void UpdateTransformationMatrix()
//     {
//         Matrix4x4 currentTransformationMatrix = Matrix4x4.TRS(
//             controlCube.transform.position,
//             controlCube.transform.rotation,
//             controlCube.transform.localScale
//         );
//         if (currentTransformationMatrix != transformationMatrix)
//         {
//             transformationMatrix = currentTransformationMatrix;
//             spaceDataManager.UpdateTransformationMatrix(transformationMatrix);
//         }
//     }

//     public void Quit()
//     {
//         Application.Quit();
//     }
// }