using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Jobs;

namespace VoxelEngine.VoxelManager
{
	public static partial class VoxelInstanceGeneration
	{
		public static void FillSwizzledStructure(
			NativeArray<uint> _structure,
			NativeArray<uint> _swizzledStructure
		)
		{
			FillSwizzledStructureJob job = new FillSwizzledStructureJob
			{
				structure = _structure,
				swizzledStructure = _swizzledStructure
			};

			JobHandle handle = job.Schedule(6, 1); // 6 faces
			handle.Complete();
		}

		[BurstCompile]
		struct FillSwizzledStructureJob : IJobParallelFor
		{
			[ReadOnly] public NativeArray<uint> structure;

			[NativeDisableParallelForRestriction]
			public NativeArray<uint> swizzledStructure;

			public void Execute(int direction)
			{
				NativeSlice<uint> structureSlice
				= new NativeSlice<uint>(swizzledStructure, direction * 1024, 1024);

				NativeSlice<uint> swizzledSlice
				= new NativeSlice<uint>(swizzledStructure, direction * 1024, 1024);

				for (int i = 0; i < structureSlice.Length; i++)
				{
					int crossPos0 = i % 32;
					int crossPos1 = i / 32;

					int structureIndex = crossPos0 + crossPos1 * 32;
					uint stack = structureSlice[structureIndex];

					while (stack != 0)
					{
						int biPos = math.tzcnt(stack);

						int3 swizzledPos = SwizzleIndex(biPos, crossPos0, crossPos1);

						stack &= stack - 1; // remove least bit

						if (biPos < 32)
						{
							int swizzledIndex = swizzledPos.y + swizzledPos.z * 32;
							swizzledSlice[swizzledIndex] |= 1u << swizzledPos.x;
						}
					}
				}
			}
		}
	}
}