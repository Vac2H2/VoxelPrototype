using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using System.Runtime.CompilerServices;

namespace VoxelEngine.VoxelManager
{
	public static partial class VoxelInstanceGeneration
	{
		public static NativeArray<uint> CreateStructure()
		{
			int size = 6 * 32 * 32;
			NativeArray<uint> structure = new NativeArray<uint>(
				size,
				Allocator.Persistent,
				NativeArrayOptions.ClearMemory
			);
			return structure;
		}

		// returns (biPos, crossPos0, crossPos1)
		public static int3 SwitchDirection(int x, int y, int z, int d)
		{
			return d switch
			{
				// X
				0 or 1 => new int3(x, y, z), // swizzled -> (y, x, z) -> crossPos1 will be the height
											 // Y
				2 or 3 => new int3(y, x, z), // swizzled -> (x, y, z) -> crossPos1
											 // Z
				4 or 5 => new int3(z, x, y), // swizzled -> (x, z, y) -> crossPos1
				_ => new int3(0, 0, 0)
			};
		}

		// from (biPos, crossPos0, crossPos1) to (x, y, z)
		[BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int3 SwitchToStandardRepresentation(int biPos, int crossPos0, int crossPos1, int d)
		{
			return d switch
			{
				0 or 1 => new int3(crossPos0, biPos, crossPos1),
				2 or 3 => new int3(biPos, crossPos0, crossPos1),
				4 or 5 => new int3(biPos, crossPos1, crossPos0),
				_ => new int3(0, 0, 0)
			};
		}
		
		[BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ExtractVoxelState(NativeSlice<uint> voxels, int3 position)
		{
			uint mask = 1u << position.x;
			return (voxels[position.y + position.z * 32] & mask) != 0u;
		}
		
		public static int3 SwizzleIndex(int biPos, int crossPos0, int crossPos1)
		{
			return new int3(crossPos0, biPos, crossPos1);
		}
	}
}