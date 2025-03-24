using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

namespace VoxelEngine.VoxelManager
{
	/// <summary>
	/// VoxelStateManager stores chunk voxel states in CPU.
	/// </summary>
	public partial class VoxelStateManager
	{
		public const int STATE_SIZE = ChunkManager.CHUNK_WIDTH * ChunkManager.CHUNK_WIDTH;
		NativeArray<uint> state;
		public VoxelStateManager(int maxNumChunk)
		{
			state = new NativeArray<uint>(
				maxNumChunk * STATE_SIZE,
				Allocator.Persistent,
				NativeArrayOptions.ClearMemory
			);
		}
	}
}