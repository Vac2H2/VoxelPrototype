using NUnit.Framework;
using Unity.Mathematics;
using Unity.Collections;
using UnityEngine;
using UnityEngine.TestTools;

public class QuadManagerTests
{
    [Test]
    public void InitialNumberOfAvailableBlocks()
    {
        int maxNumChunk = 100;
        int maxNumBlock = 100;
        QuadManager quadManager = new QuadManager(maxNumChunk, maxNumBlock);

        int number = quadManager.GetNumOfAvailableBlocks();

        Assert.AreEqual(100, number);

        quadManager.DestroyBasicStructures();
    }

    [Test]
    public void AddChunk()
    {
        int maxNumChunk = 100;
        int maxNumBlock = 100;
        QuadManager quadManager = new QuadManager(maxNumChunk, maxNumBlock);

        int3 chunkPosition = new int3(0, 0, 0);
        quadManager.AddChunk(chunkPosition);

        bool isExisted = quadManager.CheckChunkExistsInChunkToBlocksMap(chunkPosition);
        Assert.IsTrue(isExisted);

        quadManager.DestroyBasicStructures();
    }

    [Test]
    public void UploadInstanceFirstTime()
    {
        int maxNumChunk = 100;
        int maxNumBlock = 100;
        QuadManager quadManager = new QuadManager(maxNumChunk, maxNumBlock);

        int3 chunkPosition = new int3(0, 0, 0);
        quadManager.AddChunk(chunkPosition);

        int dummySize = 256;
        VoxelQuadsGeneration.InstanceData quadData = new VoxelQuadsGeneration.InstanceData
        {
            EncodedQuadData = 233u,
            Direction = 1u
        };
        NativeList<VoxelQuadsGeneration.InstanceData> instList
        = QuadManager.GenerateDummyInstances(dummySize, quadData);

        quadManager.UpdateInstance(chunkPosition, 0, instList);
        bool allMatch = quadManager.CheckInstanceBufferRange(quadData, 0, dummySize);
        Assert.IsTrue(allMatch);

        instList.Dispose();
        quadManager.DestroyBasicStructures();
    }

    [Test]
    public void UploadInstanceSecondTime()
    {
        int maxNumChunk = 100;
        int maxNumBlock = 100;
        QuadManager quadManager = new QuadManager(maxNumChunk, maxNumBlock);

        int3 chunkPosition = new int3(0, 0, 0);
        quadManager.AddChunk(chunkPosition);

        // first time
        int dummySize = 256;
        VoxelQuadsGeneration.InstanceData quadData = new VoxelQuadsGeneration.InstanceData
        {
            EncodedQuadData = 233u,
            Direction = 1u
        };
        NativeList<VoxelQuadsGeneration.InstanceData> instList
        = QuadManager.GenerateDummyInstances(dummySize, quadData);
        quadManager.UpdateInstance(chunkPosition, 0, instList);
        instList.Dispose();

        // second time
        dummySize = 756;
        quadData = new VoxelQuadsGeneration.InstanceData
        {
            EncodedQuadData = 234u,
            Direction = 2u
        };
        instList
        = QuadManager.GenerateDummyInstances(dummySize, quadData);
        quadManager.UpdateInstance(chunkPosition, 0, instList);
        bool allMatch = quadManager.CheckInstanceBufferRange(quadData, 0, dummySize);

        Assert.IsTrue(allMatch);

        instList.Dispose();
        quadManager.DestroyBasicStructures();
    }

    [Test]
    public void UploadTwoChunksInstance()
    {
        int maxNumChunk = 100;
        int maxNumBlock = 100;
        QuadManager quadManager = new QuadManager(maxNumChunk, maxNumBlock);

        int3 chunkPosition0 = new int3(0, 0, 0);
        quadManager.AddChunk(chunkPosition0);
        int3 chunkPosition1 = new int3(1, 0, 0);
        quadManager.AddChunk(chunkPosition1);

        // first chunk
        int dummySize0 = 256;
        VoxelQuadsGeneration.InstanceData quadData = new VoxelQuadsGeneration.InstanceData
        {
            EncodedQuadData = 233u,
            Direction = 1u
        };
        NativeList<VoxelQuadsGeneration.InstanceData> instList
        = QuadManager.GenerateDummyInstances(dummySize0, quadData);
        quadManager.UpdateInstance(chunkPosition0, 0, instList);
        bool allMatch = quadManager.CheckInstanceBufferRange(quadData, 0, dummySize0);
        Assert.IsTrue(allMatch);
        instList.Dispose();

        // second chunk
        int dummySize1 = 756;
        quadData = new VoxelQuadsGeneration.InstanceData
        {
            EncodedQuadData = 234u,
            Direction = 2u
        };
        instList
        = QuadManager.GenerateDummyInstances(dummySize1, quadData);
        quadManager.UpdateInstance(chunkPosition1, 0, instList);
        int BLOCK_SIZE = 1024;
        allMatch = quadManager.CheckInstanceBufferRange(quadData, BLOCK_SIZE, dummySize1);
        Assert.IsTrue(allMatch);
        instList.Dispose();

        quadManager.DestroyBasicStructures();
    }

    [Test]
    public void UploadTwoChunksInstanceWithResize()
    {
        int maxNumChunk = 100;
        int maxNumBlock = 100;
        QuadManager quadManager = new QuadManager(maxNumChunk, maxNumBlock);

        int3 chunkPosition0 = new int3(0, 0, 0);
        quadManager.AddChunk(chunkPosition0);
        int3 chunkPosition1 = new int3(1, 0, 0);
        quadManager.AddChunk(chunkPosition1);

        // first chunk
        int dummySize0 = 256;
        VoxelQuadsGeneration.InstanceData quadData0 = new VoxelQuadsGeneration.InstanceData
        {
            EncodedQuadData = 233u,
            Direction = 1u
        };
        NativeList<VoxelQuadsGeneration.InstanceData> instList
        = QuadManager.GenerateDummyInstances(dummySize0, quadData0);
        quadManager.UpdateInstance(chunkPosition0, 0, instList);
        instList.Dispose();

        // second chunk
        int dummySize1 = 756;
        VoxelQuadsGeneration.InstanceData quadData1 = new VoxelQuadsGeneration.InstanceData
        {
            EncodedQuadData = 234u,
            Direction = 2u
        };
        instList
        = QuadManager.GenerateDummyInstances(dummySize1, quadData1);
        quadManager.UpdateInstance(chunkPosition1, 0, instList);
        instList.Dispose();

        // upload first chunk with larger number of instances
        int dummySize2 = 1048;
        instList
        = QuadManager.GenerateDummyInstances(dummySize2, quadData0);
        quadManager.UpdateInstance(chunkPosition0, 0, instList);
        instList.Dispose();

        int BLOCK_SIZE = 1024;
        Assert.IsTrue(quadManager.CheckInstanceBufferRange(quadData0, 0, BLOCK_SIZE));
        int elementLeft = 1048 - 1024;
        Assert.IsTrue(quadManager.CheckInstanceBufferRange(quadData0, 2 * BLOCK_SIZE, elementLeft));
        Assert.IsTrue(quadManager.CheckInstanceBufferRange(quadData1, BLOCK_SIZE, dummySize1));
        quadManager.DestroyBasicStructures();
    }
}
