using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using VoxelEngine.VoxelManager;
using VoxelEngine.VoxelRendering;

namespace VoxelEngine.Simulator
{
	public partial class Simulator
	{
		public Simulator(int maxNumChunk, int maxNumBlockPerChunk, int gpuBlockSize, float3 scale)
		{
			ChunkManager chunkManager
			= new ChunkManager(maxNumChunk, maxNumBlockPerChunk, gpuBlockSize, scale);

			(
				float3 _,
				ComputeBuffer instanceBuffer,
				ComputeBuffer instanceCountBuffer,
				ComputeBuffer chunkPositionBuffer,
				ComputeBuffer typeIndexBuffer,
				ComputeBuffer typeBuffer,
				ComputeBuffer visibleBlockBuffer,
				ComputeBuffer visibleBlockCountBuffer
			) = chunkManager.GetRendererData();

			VoxelRenderer voxelRenderer = new VoxelRenderer
			(
				scale,
				instanceBuffer,
				instanceCountBuffer,
				chunkPositionBuffer,
				typeIndexBuffer,
				typeBuffer,
				visibleBlockBuffer,
				visibleBlockCountBuffer
			);
		}
	}
}