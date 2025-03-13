using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

public class VisualDataUpdator : MonoBehaviour
{
    ChunkVoxelManager chunkVoxelManager;
    QuadManager quadManager;
    void Start()
    {
        
    }

    void Update()
    {
        NativeQueue<int3> dirtyChunksQueue = chunkVoxelManager.GetDirtyChunkQueue();
        int count = dirtyChunksQueue.Count;

        for (int i = 0; i < count; i++)
        {
            int3 chunkPosition = dirtyChunksQueue.Dequeue();

            (bool found, NativeSlice<uint> state, NativeSlice<uint> type)
            = chunkVoxelManager.GetChunkSlice(chunkPosition);

            NativeList<VoxelQuadsGeneration.InstanceData> instanceList
            = VoxelQuadsGeneration.GreedyQuadsPipeline(state);

            int chunkIndex = chunkVoxelManager.GetChunkIndex(chunkPosition);
            quadManager.UpdateInstance(chunkPosition, chunkIndex, instanceList);
            chunkVoxelManager.UpdateTypeBuffer(chunkPosition);

            instanceList.Dispose();
        }
    }
}
