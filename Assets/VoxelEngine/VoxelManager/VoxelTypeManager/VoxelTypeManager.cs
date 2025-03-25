using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using PlasticGui.WorkspaceWindow;
using Unity.Collections.LowLevel.Unsafe;

namespace VoxelEngine.VoxelManager
{
	/// <summary>
	/// VoxelTypeManager stores chunk voxel types in GPU.
	/// </summary>
	public partial class VoxelTypeManager
	{
		public const int TYPE_SIZE
		= ChunkManager.CHUNK_WIDTH * ChunkManager.CHUNK_WIDTH * ChunkManager.CHUNK_WIDTH;

		ComputeBuffer typeBuffer;

		public VoxelTypeManager(int maxNumChunk)
		{
			typeBuffer = new ComputeBuffer(
				maxNumChunk * TYPE_SIZE,
				sizeof(uint),
				ComputeBufferType.Structured
			);
		}

		public ComputeBuffer GetTypeBuffer()
		{
			return typeBuffer;
		}

		public void Dispose()
		{
			typeBuffer.Dispose();
		}
	}
}