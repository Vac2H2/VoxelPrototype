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

        public static int CountConsecutiveZeros(uint stack)
        {
            return math.tzcnt(stack);
        }

        public static int CountConsecutiveOnes(uint stack)
        {
            return math.tzcnt(~stack);
        }

        public static int2 FindStartAndEnd(uint stack)
        {
            int start = CountConsecutiveZeros(stack);
            stack >>= start;
            int end = CountConsecutiveOnes(stack);

            return new int2(start, start + end);
        }

        public static uint GenerateRangeMask(int2 range)
        {
            // return ((1u << (range.y - range.x)) - 1) << range.x;

            int width = range.y - range.x;
            if (width == 32)
                return uint.MaxValue; // All 32 bits are set to 1.
            return ((1u << width) - 1) << range.x;
        }

        public static uint ClearBits(uint stack, uint mask)
        {
            return stack & (~mask);
        }

        public struct QuadData
        {
            public uint x;
            public uint y;
            public uint z;
            public uint xScale;
            public uint yScale;
            public uint zScale;
        }

        public static uint EncodeQuad(QuadData quad)
        {
            uint data = 0u;
            data |= (quad.x & 0x1F) << 0;  // bits 0..4  (5 bits)
            data |= (quad.y & 0x1F) << 5;  // bits 5..9  (5 bits)
            data |= (quad.z & 0x1F) << 10; // bits 10..14 (5 bits)
            data |= (quad.xScale & 0x1F) << 15; // bits 15..19 (5 bits)
            data |= (quad.yScale & 0x1F) << 20; // bits 20..24 (5 bits)
            data |= (quad.zScale & 0x1F) << 25; // bits 25..29 (5 bits)
            return data;
        }

        public static QuadData DecodeQuad(uint data)
        {
            QuadData result;
            result.x = (data >> 0) & 0x1F;
            result.y = (data >> 5) & 0x1F;
            result.z = (data >> 10) & 0x1F;
            result.xScale = ((data >> 15) & 0x1F) + 1;
            result.yScale = ((data >> 20) & 0x1F) + 1;
            result.zScale = ((data >> 25) & 0x1F) + 1;
            return result;
        }
        
        public static void AddVoxel(NativeArray<uint> voxels, int x, int y, int z)
        {
            uint mask = 1u << x;
            voxels[y + z * 32] |= mask;
        }
	}
}