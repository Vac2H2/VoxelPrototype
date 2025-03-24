using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Jobs;

namespace VoxelEngine.VoxelManager
{
	// [BurstCompile]
	public static partial class VoxelInstanceGeneration
	{
		public static void FillStructure(NativeSlice<uint> _voxels, NativeArray<uint> _structure)
		{
			// FillChunkOnlyBurst(_voxels, _structure);
			FillStructureJob job = new FillStructureJob
			{
				voxels = _voxels,
				structure = _structure
			};

			JobHandle handle = job.Schedule(6, 1); // 6 faces
			handle.Complete();
		}

		[BurstCompile]
		struct FillStructureJob : IJobParallelFor
		{
			[ReadOnly] public NativeSlice<uint> voxels;

			[NativeDisableParallelForRestriction]
			public NativeArray<uint> structure; // [6 * 32 * 32] for 6 directions

			public void Execute(int direction)
			{
				NativeSlice<uint> slice = new NativeSlice<uint>(structure, direction * 1024, 1024);

				for (int i = 0; i < slice.Length; i++)
				{
					int crossPos0 = i % 32;
					int crossPos1 = i / 32;

					int structureIndex = direction + crossPos0 * 6 + crossPos1 * 6 * 32;

					for (int biPos = 0; biPos < 32; biPos++)
					{
						int3 position
						= SwitchToStandardRepresentation(biPos, crossPos0, crossPos1, direction);

						if (ExtractVoxelState(voxels, position))
						{
							structure[structureIndex] |= 1u << biPos;
						}
					}

					uint stack = structure[structureIndex];
					if (direction % 2 == 0) // even is inward face
					{
						structure[structureIndex] = stack & ~(stack >> 1);
					}
					else // odd is outward face
					{
						structure[structureIndex] = stack & ~(stack << 1);
					}
				}
			}
		}
	}
}