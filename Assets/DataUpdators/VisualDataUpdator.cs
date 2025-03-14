using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

public class VisualDataUpdator {
    VoxelDataManager voxelDataManager;
    QuadInstanceManager quadInstanceManager;

    public VisualDataUpdator(VoxelDataManager _voxelDataManager, QuadInstanceManager _quadInstanceManager)
    {
        voxelDataManager = _voxelDataManager;
        quadInstanceManager = _quadInstanceManager;
    }

    public void Update()
    {
        NativeQueue<int3> dirtyChunksQueue = voxelDataManager.GetDirtyChunkQueue();
        int count = dirtyChunksQueue.Count;

        for (int i = 0; i < count; i++)
        {
            int3 chunkPosition = dirtyChunksQueue.Dequeue();

            (bool found, NativeSlice<uint> state, NativeSlice<uint> type)
            = voxelDataManager.GetChunkSlice(chunkPosition);

            NativeList<VoxelQuadsGeneration.InstanceData> instanceList
            = VoxelQuadsGeneration.GreedyQuadsPipeline(state);

            int chunkIndex = voxelDataManager.GetChunkIndex(chunkPosition);
            quadInstanceManager.UpdateInstance(chunkPosition, chunkIndex, instanceList);
            voxelDataManager.UpdateTypeBuffer(chunkPosition);

            instanceList.Dispose();
        }
    }
}
