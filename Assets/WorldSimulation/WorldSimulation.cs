using UnityEngine;

public class WorldSimulation : MonoBehaviour
{
    public int MaxNumberChunk;
    public int MaxNumberBlock;
    ChunkVoxelManager chunkVoxelManager;
    QuadManager quadManager;
    VisualDataUpdator visualDataUpdator;
    void Start()
    {
        chunkVoxelManager = new ChunkVoxelManager(MaxNumberChunk);
        quadManager = new QuadManager(MaxNumberChunk, MaxNumberBlock);
        visualDataUpdator = new VisualDataUpdator(chunkVoxelManager, quadManager);
    }

    void Update()
    {
        visualDataUpdator.Update();
    }
}
