using UnityEngine;

public class WorldSimulation : MonoBehaviour
{
    public int MaxNumberChunk;
    public int MaxNumberBlock;
    ChunkVoxelManager chunkVoxelManager;
    QuadManager quadManager;
    VisualDataUpdator visualDataUpdator;
    QuadRenderer quadRenderer;
    void Start()
    {
        chunkVoxelManager = new ChunkVoxelManager(MaxNumberChunk);
        quadManager = new QuadManager(MaxNumberChunk, MaxNumberBlock);
        visualDataUpdator = new VisualDataUpdator(chunkVoxelManager, quadManager);

        (
            ComputeBuffer instanceBuffer,
            ComputeBuffer instanceCountBuffer,
            ComputeBuffer chunkPositionBuffer,
            ComputeBuffer typeIndexBuffer
        ) = quadManager.GetRenderBuffers();
        ComputeBuffer typeBuffer = chunkVoxelManager.GetTypeBuffer();

        quadRenderer = new QuadRenderer(
            instanceBuffer,
            instanceCountBuffer,
            chunkPositionBuffer,
            typeIndexBuffer,
            typeBuffer
        );

        VoxelDataUpdator.CreateBasicScene(chunkVoxelManager);
    }

    void Update()
    {
        visualDataUpdator.Update();
        quadRenderer.Update();
    }

    void OnDestroy()
    {
        chunkVoxelManager.DestroyBasicStructures();
        quadManager.DestroyBasicStructures();
        quadRenderer.Destroy();
    }
}
