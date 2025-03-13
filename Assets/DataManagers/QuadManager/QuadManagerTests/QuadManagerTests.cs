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
}
