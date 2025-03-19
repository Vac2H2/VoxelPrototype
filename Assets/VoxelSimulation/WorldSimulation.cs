using UnityEngine;
using Unity.Mathematics;

public class WorldSimulation : MonoBehaviour
{
    public int MaxNumberChunk;
    public int MaxNumberBlock;
    ChunkVoxelManager chunkVoxelManager;
    QuadManager quadManager;
    VisualDataUpdator visualDataUpdator;
    QuadRenderer quadRenderer;

    Matrix4x4 transformationMatrix;

    void Start()
    {
        transformationMatrix = transform.localToWorldMatrix;

        chunkVoxelManager = new ChunkVoxelManager(MaxNumberChunk, transformationMatrix);
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
            typeBuffer,
            transformationMatrix
        );

        VoxelDataUpdator.CreateBasicScene(
            chunkVoxelManager,
            quadManager,
            new int3(-2, 0, -2),
            new int3(4, 1, 4),
            2u
        );

        VoxelDataUpdator.AddStaircase(
            chunkVoxelManager,
            quadManager,
            new int3(0, 32, 0),
            new int3(4, 1, 4),
            3u
        );

        // VoxelDataUpdator.AddRectangle(
        //     chunkVoxelManager,
        //     quadManager,
        //     new int3(5, 32, 5),
        //     new int3(8, 8, 8)
        // );
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

    public VoxelDataManager GetVoxelDataManager()
    {
        return chunkVoxelManager;
    }
}
