using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

public partial class QuadManager : QuadInstanceManager
{
	public QuadManager(int maxNumChunk, int maxNumBlock) : base(maxNumChunk, maxNumBlock)
	{

	}

	public ComputeBuffer GetInstanceBuffer()
	{
		return instanceBuffer;
	}

	public ComputeBuffer GetChunkPositionBuffer()
	{
		return chunkPositionBuffer;
	}

	public ComputeBuffer GetInstanceCountBufferBuffer()
	{
		return instanceCountBuffer;
	}

	public int GetNumOfAvailableBlocks()
	{
		return availableBlockIndex.Count;
	}

	public bool CheckChunkExistsInChunkToBlocksMap(int3 chunkPosition)
	{
		if (chunkToBlocksMap.ContainsKey(chunkPosition))
		{
			return true;
		}
		return false;
	}
}