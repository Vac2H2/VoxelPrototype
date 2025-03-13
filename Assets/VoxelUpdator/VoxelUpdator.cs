// using Unity.Collections;
// using Unity.Mathematics;
// using Unity.VisualScripting;
// using UnityEngine;

// public partial class VoxelUpdator : MonoBehaviour
// {
//     public struct VoxelUpdateData
//     {
//         public int3 Position;
//         public uint Type;
//     }

//     SpaceDataManagerOptimized dataManager;
//     NativeQueue<VoxelUpdateData> updateQueue;
//     void Start()
//     {
//         dataManager = GetComponentInParent<SpaceDataManager>().GetSpaceDataManager();
//         updateQueue = new NativeQueue<VoxelUpdateData>(Allocator.Persistent);
//     }

//     void Update()
//     {
//         int count = updateQueue.Count;
//         if (count == 0)
//         {
//             return;
//         }

//         NativeHashSet<int3> dirtyChunks = new NativeHashSet<int3>(100, Allocator.Persistent);
//         for (int i = 0; i < count; i++)
//         {
//             VoxelUpdateData data = updateQueue.Dequeue();
//             GetChunkAndLocalPosition(data.Position, out int3 chunkPos, out int3 localPos);
//             NativeSlice<uint> slice;
//             if (!dataManager.GetChunkStateSlice(chunkPos, out slice))
//             {
//                 NativeArray<uint> state = new NativeArray<uint>(32 * 32, Allocator.Persistent);
//                 NativeArray<uint> type = new NativeArray<uint>(32 * 32 * 32, Allocator.Persistent);
//                 dataManager.AddChunk(chunkPos, state, type);

//                 state.Dispose();
//                 type.Dispose();

//                 dataManager.GetChunkStateSlice(chunkPos, out slice);
//             }
//             if (data.Type == 99u)
//             {
//                 Debug.Log("remove");
//                 RemoveVoxel(slice, localPos.x, localPos.y, localPos.z);
//             }
//             else
//             {
//                 Debug.Log("add");
//                 AddVoxel(slice, localPos.x, localPos.y, localPos.z);
//                 dataManager.GetChunkTypeSlice(chunkPos, out NativeSlice<uint> typeSlice);
//                 typeSlice[localPos.x + 32 * localPos.y + 32 * 32 * localPos.z] = data.Type;
//             }

//             dirtyChunks.Add(chunkPos);
//         }

//         foreach (int3 chunkPos in dirtyChunks)
//         {
//             dataManager.SetChunkDirty(chunkPos);
//         }

//         dirtyChunks.Dispose();
//     }

//     void OnDestroy()
//     {
//         updateQueue.Dispose();
//     }

//     public void UpdateVoxel(int3 position, uint type)
//     {
//         updateQueue.Enqueue(new VoxelUpdateData
//         {
//             Position = position,
//             Type = type
//         });
//     }

//     public void RemoveVoxelByBox(int3 startPos, int3 endPos)
//     {
//         int3 iterStart = int3.zero;
//         int3 iterEnd = int3.zero;
//         for (int i = 0; i < 3; i++)
//         {
//             int min = math.min(startPos[i], endPos[i]);
//             int max = math.max(startPos[i], endPos[i]);
//             iterStart[i] = min;
//             iterEnd[i] = max;
//         }

//         GetChunkAndLocalPosition(iterStart, out int3 chunkPosStart, out int3 localPosStart);
//         GetChunkAndLocalPosition(iterEnd, out int3 chunkPosEnd, out int3 localPosEnd);
//         NativeList<int2> xRanges = GetChunkRanges(localPosStart.x, iterEnd.x - iterStart.x + 1);
//         NativeList<int2> yRanges = GetChunkRanges(localPosStart.y, iterEnd.y - iterStart.y + 1);
//         NativeList<int2> zRanges = GetChunkRanges(localPosStart.z, iterEnd.z - iterStart.z + 1);

//         int3 scales = chunkPosEnd - chunkPosStart + 1;
//         for (int x = 0; x < scales.x; x++)
//         {
//             for (int y = 0; y < scales.y; y++)
//             {
//                 for (int z = 0; z < scales.z; z++)
//                 {
//                     int3 chunkPos = chunkPosStart + new int3(x, y, z);
//                     if (dataManager.GetChunkStateSlice(chunkPos, out NativeSlice<uint> slice))
//                     {
//                         ClearStateByRange(slice, xRanges[x], yRanges[y], zRanges[z]);
//                         dataManager.SetChunkDirty(chunkPos);
//                     }
//                 }
//             }
//         }

//         xRanges.Dispose();
//         yRanges.Dispose();
//         zRanges.Dispose();
//     }

//     public void AddVoxelByBox(int3 startPos, int3 endPos, uint type)
//     {
//         int3 iterStart = int3.zero;
//         int3 iterEnd = int3.zero;
//         for (int i = 0; i < 3; i++)
//         {
//             int min = math.min(startPos[i], endPos[i]);
//             int max = math.max(startPos[i], endPos[i]);
//             iterStart[i] = min;
//             iterEnd[i] = max;
//         }

//         GetChunkAndLocalPosition(iterStart, out int3 chunkPosStart, out int3 localPosStart);
//         GetChunkAndLocalPosition(iterEnd, out int3 chunkPosEnd, out int3 localPosEnd);
//         NativeList<int2> xRanges = GetChunkRanges(localPosStart.x, iterEnd.x - iterStart.x + 1);
//         NativeList<int2> yRanges = GetChunkRanges(localPosStart.y, iterEnd.y - iterStart.y + 1);
//         NativeList<int2> zRanges = GetChunkRanges(localPosStart.z, iterEnd.z - iterStart.z + 1);

//         int3 scales = chunkPosEnd - chunkPosStart + 1;
//         for (int x = 0; x < scales.x; x++)
//         {
//             for (int y = 0; y < scales.y; y++)
//             {
//                 for (int z = 0; z < scales.z; z++)
//                 {
//                     int3 chunkPos = chunkPosStart + new int3(x, y, z);
//                     if (!dataManager.GetChunkStateSlice(chunkPos, out NativeSlice<uint> stateSlice))
//                     { // chunks not found -> add

//                         NativeArray<uint> stateArray
//                         = new NativeArray<uint>(32 * 32, Allocator.Persistent);
//                         NativeArray<uint> typeArray
//                         = new NativeArray<uint>(32 * 32 * 32, Allocator.Persistent);

//                         dataManager.AddChunk(chunkPos, stateArray, typeArray);

//                         stateArray.Dispose();
//                         typeArray.Dispose();
//                     }

//                     dataManager.GetChunkStateSlice(chunkPos, out stateSlice);
//                     dataManager.GetChunkTypeSlice(chunkPos, out NativeSlice<uint> typeSlice);
//                     AddStateByRange(stateSlice, xRanges[x], yRanges[y], zRanges[z]);
//                     UpdateTypeByRange(typeSlice, xRanges[x], yRanges[y], zRanges[z], type);
//                     dataManager.SetChunkDirty(chunkPos);
//                 }
//             }
//         }

//         xRanges.Dispose();
//         yRanges.Dispose();
//         zRanges.Dispose();
//     }

//     public void FillVoxelByBox(int3 startPos, int3 endPos, uint type)
//     {
//         int3 iterStart = int3.zero;
//         int3 iterEnd = int3.zero;
//         for (int i = 0; i < 3; i++)
//         {
//             int min = math.min(startPos[i], endPos[i]);
//             int max = math.max(startPos[i], endPos[i]);
//             iterStart[i] = min;
//             iterEnd[i] = max;
//         }

//         GetChunkAndLocalPosition(iterStart, out int3 chunkPosStart, out int3 localPosStart);
//         GetChunkAndLocalPosition(iterEnd, out int3 chunkPosEnd, out int3 localPosEnd);
//         NativeList<int2> xRanges = GetChunkRanges(localPosStart.x, iterEnd.x - iterStart.x + 1);
//         NativeList<int2> yRanges = GetChunkRanges(localPosStart.y, iterEnd.y - iterStart.y + 1);
//         NativeList<int2> zRanges = GetChunkRanges(localPosStart.z, iterEnd.z - iterStart.z + 1);

//         int3 scales = chunkPosEnd - chunkPosStart + 1;
//         for (int x = 0; x < scales.x; x++)
//         {
//             for (int y = 0; y < scales.y; y++)
//             {
//                 for (int z = 0; z < scales.z; z++)
//                 {
//                     int3 chunkPos = chunkPosStart + new int3(x, y, z);
//                     if (!dataManager.GetChunkStateSlice(chunkPos, out NativeSlice<uint> stateSlice))
//                     { // chunks not found -> add

//                         NativeArray<uint> stateArray
//                         = new NativeArray<uint>(32 * 32, Allocator.Persistent);
//                         NativeArray<uint> typeArray
//                         = new NativeArray<uint>(32 * 32 * 32, Allocator.Persistent);

//                         dataManager.AddChunk(chunkPos, stateArray, typeArray);

//                         stateArray.Dispose();
//                         typeArray.Dispose();
//                     }

//                     dataManager.GetChunkStateSlice(chunkPos, out stateSlice);
//                     dataManager.GetChunkTypeSlice(chunkPos, out NativeSlice<uint> typeSlice);

//                     NativeArray<uint> masks = FillStateByRange(stateSlice, xRanges[x], yRanges[y], zRanges[z]);
//                     UpdateTypeByRange(typeSlice, masks, xRanges[x], yRanges[y], zRanges[z], type);
//                     dataManager.SetChunkDirty(chunkPos);

//                     masks.Dispose();
//                 }
//             }
//         }

//         xRanges.Dispose();
//         yRanges.Dispose();
//         zRanges.Dispose();
//     }
// }
