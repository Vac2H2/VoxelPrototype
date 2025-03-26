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

        VoxelDataUpdator.AddStaircase(
            chunkVoxelManager,
            quadManager,
            new int3(4, 36, 0),
            new int3(4, 1, 4),
            3u
        );

        VoxelDataUpdator.AddRectangle(
            chunkVoxelManager,
            quadManager,
            new int3(5, 32, 5),
            new int3(26, 26, 4),
            4u
        );

        VoxelDataUpdator.AddRectangle(
            chunkVoxelManager,
            quadManager,
            new int3(0, 32, 0),
            new int3(8, 1, 8),
            5u
        );

        VoxelDataUpdator.AddRectangle(
            chunkVoxelManager,
            quadManager,
            new int3(13, 31, 12),
            new int3(8, 1, 8),
            4u
        );

        VoxelDataUpdator.AddRectangle(
            chunkVoxelManager,
            quadManager,
            new int3(6, 31, 7),
            new int3(8, 2, 8),
            3u
        );

        VoxelDataUpdator.AddRectangle(
            chunkVoxelManager,
            quadManager,
            new int3(20, 32, -16),
            new int3(1, 8, 4),
            3u
        );

        VoxelDataUpdator.AddRectangle(
            chunkVoxelManager,
            quadManager,
            new int3(20, 32, -24),
            new int3(1, 8, 4),
            3u
        );

        VoxelDataUpdator.AddRectangle(
            chunkVoxelManager,
            quadManager,
            new int3(20, 32, -30),
            new int3(1, 8, 4),
            3u
        );

        // floor case
        VoxelDataUpdator.AddRectangle(
            chunkVoxelManager,
            quadManager,
            new int3(0, 32, -16),
            new int3(1, 8, 4),
            3u
        );
        VoxelDataUpdator.AddRectangle(
            chunkVoxelManager,
            quadManager,
            new int3(0, 40, -20),
            new int3(1, 1, 4),
            3u
        );

        VoxelDataUpdator.AddRectangle(
            chunkVoxelManager,
            quadManager,
            new int3(0, 32, -24),
            new int3(1, 8, 4),
            3u
        );

        VoxelDataUpdator.AddRectangle(
            chunkVoxelManager,
            quadManager,
            new int3(0, 32, -32),
            new int3(1, 8, 4),
            3u
        );

        VoxelDataUpdator.AddRectangle(
            chunkVoxelManager,
            quadManager,
            new int3(8, 39, -8),
            new int3(8, 1, 8),
            3u
        );

        VoxelDataUpdator.AddRectangle(
            chunkVoxelManager,
            quadManager,
            new int3(16, 48, -16),
            new int3(4, 1, 4),
            3u
        );
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
