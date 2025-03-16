using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

public partial class ChunkVoxelManager : VoxelDataManager
{
	public ChunkVoxelManager(int maxNumChunk, Matrix4x4 localToWorld) : base(maxNumChunk, localToWorld)
	{

	}
}