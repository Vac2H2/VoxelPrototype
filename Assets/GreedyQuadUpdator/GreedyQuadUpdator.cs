// using UnityEngine;
// using Unity.Collections;
// using Unity.Mathematics;

// public class GreedyQuadUpdator : MonoBehaviour
// {
//     SpaceDataManagerOptimized dataManager;
//     void Start()
//     {
//         dataManager = GetComponentInParent<SpaceDataManager>().GetSpaceDataManager();
//     }

//     void Update()
//     {
//         NativeQueue<int3> dirtyChunks = dataManager.GetDirtyChunks();
//         int count = dirtyChunks.Count;
//         for (int i = 0; i < count; i++)
//         {
//             NativeSlice<uint> stateSlice;
//             int3 chunkPosition = dirtyChunks.Dequeue();
//             if (dataManager.GetChunkStateSlice(chunkPosition, out stateSlice))
//             {
//                 NativeList<VoxelQuadsGeneration.InstanceData> instanceList
//                 = VoxelQuadsGeneration.GreedyQuadsPipeline(stateSlice);

//                 // for (int j = 0; j < instanceList.Length; j++)
//                 // {
//                 //     VoxelQuadsGeneration.QuadData data
//                 //     = VoxelQuadsGeneration.DecodeQuad(instanceList[j].EncodedQuadData);

//                 //     Debug.Log($"chunk {i}, instance {j}");
//                 //     Debug.Log($"x {data.x}");
//                 //     Debug.Log($"y {data.y}");
//                 //     Debug.Log($"z {data.z}");
//                 //     Debug.Log($"xScale {data.xScale}");
//                 //     Debug.Log($"yScale {data.yScale}");
//                 //     Debug.Log($"zScale {data.zScale}");
//                 // }

//                 dataManager.UpdateInstance(chunkPosition, instanceList);
//                 dataManager.UpdateType(chunkPosition);

//                 instanceList.Dispose();
//             }
//         }
//     }
// }
