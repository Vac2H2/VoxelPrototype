using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Jobs;

namespace VoxelEngine.VoxelManager
{
	public static partial class VoxelInstanceGeneration
	{
		public static NativeList<VoxelInstanceManager.InstanceData> GenerateGreedyQuads(
			NativeArray<uint> _swizzledStructure
		)
		{
			NativeList<VoxelInstanceManager.InstanceData> _instances
			= new NativeList<VoxelInstanceManager.InstanceData>(8000, Allocator.Persistent);
			GenerateGreedyQuadsJob job = new GenerateGreedyQuadsJob
			{
				swizzledStructure = _swizzledStructure,
				instances = _instances.AsParallelWriter()
			};

			JobHandle handle = job.Schedule(6, 1); // 6 faces
			handle.Complete();

			return _instances;
		}

		[BurstCompile]
		struct GenerateGreedyQuadsJob : IJobParallelFor
		{
			[NativeDisableParallelForRestriction]
			public NativeArray<uint> swizzledStructure;

			public NativeList<VoxelInstanceManager.InstanceData>.ParallelWriter instances;

			public void Execute(int direction)
			{
				NativeSlice<uint> swizzledSlice
				= new NativeSlice<uint>(swizzledStructure, direction * 1024, 1024);

				for (int crossPos0 = 0; crossPos0 < 32; crossPos0++)
				{

					int crossPos1 = 0;
					int index = crossPos0 + 32 * crossPos1;
					uint stack = swizzledSlice[index];

					while (true)
					{
						int2 range = FindStartAndEnd(stack);
						uint mask = GenerateRangeMask(range);
						stack = ClearBits(stack, mask);

						int endRow = crossPos1 + 1;
						while (endRow < 32)
						{
							int endRowIndex = crossPos0 + 32 * endRow;
							uint endRowStack = swizzledSlice[endRowIndex];

							if ((endRowStack | mask) == endRowStack)
							{
								endRow++;
								swizzledSlice[endRowIndex] = ClearBits(endRowStack, mask);
							}
							else
							{
								break;
							}
						}

						// append quad
						if (mask != 0u)
						{

							int3 standardPos = SwitchToStandardRepresentation(
								range.x,
								crossPos0,
								crossPos1,
								direction
							);

							int3 standardScale = SwitchToStandardRepresentation(
								range.y - range.x - 1,
								0,
								endRow - crossPos1 - 1,
								direction
							);

							uint encoded = EncodeQuad(new QuadData
							{
								x = (uint)standardPos.x,
								y = (uint)standardPos.y,
								z = (uint)standardPos.z,
								xScale = (uint)standardScale.x,
								yScale = (uint)standardScale.y,
								zScale = (uint)standardScale.z
							});
							instances.AddNoResize(new VoxelInstanceManager.InstanceData
							{
								EncodedQuadData = encoded,
								Direction = (uint)direction
							});

						}

						// move on to next row
						if (stack == 0u)
						{
							crossPos1++;
							if (crossPos1 < 32)
							{
								index = crossPos0 + 32 * crossPos1;
								stack = swizzledSlice[index]; // update to next row stack
							}
							else
							{
								break;
							}
						}
					}
				}
			}
		}
	}
}