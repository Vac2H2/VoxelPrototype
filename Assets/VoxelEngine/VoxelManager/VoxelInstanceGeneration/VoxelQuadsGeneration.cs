using Unity.Collections;
using Unity.Mathematics;

namespace VoxelEngine.VoxelManager
{
    public partial class VoxelQuadsGeneration
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

        public static uint3 SwitchToStandardRepresentationUnsigned(uint biPos, uint crossPos0, uint crossPos1, int d)
        {
            return d switch
            {
                0 or 1 => new uint3(crossPos0, biPos, crossPos1),
                2 or 3 => new uint3(biPos, crossPos0, crossPos1),
                4 or 5 => new uint3(biPos, crossPos1, crossPos0),
                _ => new uint3(0, 0, 0)
            };
        }

        public static int3 SwizzleIndex(int biPos, int crossPos0, int crossPos1)
        {
            return new int3(crossPos0, biPos, crossPos1);
        }

        // Check if an voxel at a position exists
        public static bool ExtractVoxelState(NativeArray<uint> voxels, int x, int y, int z)
        {
            uint mask = 1u << x;
            return (voxels[y + z * 32] & mask) != 0u;
        }

        public static bool ExtractVoxelState(NativeSlice<uint> voxels, int x, int y, int z)
        {
            uint mask = 1u << x;
            return (voxels[y + z * 32] & mask) != 0u;
        }

        // Check if an voxel at a position exists from structure of [6x32x32]
        public static bool ExtractVoxelStateFromStructure(NativeArray<uint> structure, int x, int y, int z, int d)
        {
            int3 pos = SwitchDirection(x, y, z, d);
            uint mask = 1u << pos.x;
            return (structure[d + 6 * pos.y + 6 * 32 * pos.z] & mask) != 0u;
        }

        // add voxel in native array with a position
        public static void AddVoxel(NativeArray<uint> voxels, int x, int y, int z)
        {
            uint mask = 1u << x;
            voxels[y + z * 32] |= mask;
        }

        public static void RemoveVoxel(NativeArray<uint> voxels, int x, int y, int z)
        {
            uint mask = 1u << x;
            voxels[y + z * 32] &= ~mask;
        }

        public static void FillStructureWithoutJob(
            NativeArray<uint> voxels,
            NativeArray<uint> structure
        )
        {
            // fill data
            for (int d = 0; d < 6; d++)
            {
                for (int x = 0; x < 32; x++)
                {
                    for (int y = 0; y < 32; y++)
                    {
                        for (int z = 0; z < 32; z++)
                        {
                            // map to stack position (biPos, crossPos0, crossPos1)
                            int3 structurePos = SwitchDirection(x, y, z, d);

                            // find corresponded index in stacks
                            int structureIndex = d + structurePos.y * 6 + structurePos.z * 6 * 32;

                            if (ExtractVoxelState(voxels, x, y, z))
                            {
                                structure[structureIndex] |= 1u << structurePos.x;
                            }
                        }
                    }
                }
            }

            // culling
            for (int d = 0; d < 6; d++)
            {
                for (int crossPos0 = 0; crossPos0 < 32; crossPos0++)
                {
                    for (int crossPos1 = 0; crossPos1 < 32; crossPos1++)
                    {
                        // find corresponded index in stacks
                        int structureIndex = d + crossPos0 * 6 + crossPos1 * 6 * 32;

                        uint stack = structure[structureIndex];
                        if (d % 2 == 0) // even is inward face
                            structure[structureIndex] = stack & ~(stack >> 1);
                        else // odd is outward face
                            structure[structureIndex] = stack & ~(stack << 1);
                    }
                }
            }
        }

        public static void FillStructureWithoutJob(
            NativeSlice<uint> voxels,
            NativeArray<uint> structure
        )
        {
            // fill data
            for (int d = 0; d < 6; d++)
            {
                for (int x = 0; x < 32; x++)
                {
                    for (int y = 0; y < 32; y++)
                    {
                        for (int z = 0; z < 32; z++)
                        {
                            // map to stack position (biPos, crossPos0, crossPos1)
                            int3 structurePos = SwitchDirection(x, y, z, d);

                            // find corresponded index in stacks
                            int structureIndex = d + structurePos.y * 6 + structurePos.z * 6 * 32;

                            if (ExtractVoxelState(voxels, x, y, z))
                            {
                                structure[structureIndex] |= 1u << structurePos.x;
                            }
                        }
                    }
                }
            }

            // culling
            for (int d = 0; d < 6; d++)
            {
                for (int crossPos0 = 0; crossPos0 < 32; crossPos0++)
                {
                    for (int crossPos1 = 0; crossPos1 < 32; crossPos1++)
                    {
                        // find corresponded index in stacks
                        int structureIndex = d + crossPos0 * 6 + crossPos1 * 6 * 32;

                        uint stack = structure[structureIndex];
                        if (d % 2 == 0) // even is inward face
                            structure[structureIndex] = stack & ~(stack >> 1);
                        else // odd is outward face
                            structure[structureIndex] = stack & ~(stack << 1);
                    }
                }
            }
        }

        public static void FillSwizzledStructureWithoutJob(
            NativeArray<uint> structure,
            NativeArray<uint> swizzledStructure
        )
        {
            for (int face = 0; face < 6; face++)
            {
                for (int crossPos0 = 0; crossPos0 < 32; crossPos0++)
                {
                    for (int crossPos1 = 0; crossPos1 < 32; crossPos1++)
                    {

                        int structureIndex = face + crossPos0 * 6 + crossPos1 * 6 * 32;
                        uint stack = structure[structureIndex];

                        while (stack != 0)
                        {
                            int biPos = math.tzcnt(stack);

                            int3 swizzledPos = SwizzleIndex(biPos, crossPos0, crossPos1);

                            stack &= stack - 1; // remove least bit

                            if (biPos < 32)
                            {
                                int swizzledIndex = face + swizzledPos.y * 6 + swizzledPos.z * 6 * 32;
                                swizzledStructure[swizzledIndex] |= 1u << swizzledPos.x;
                            }
                        }
                    }
                }
            }
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

        public struct InstanceData
        {
            public uint EncodedQuadData;
            public uint Direction;
        }

        public static NativeList<InstanceData> GenerateGreedyQuads(NativeArray<uint> swizzledStructure)
        {
            int initialSize = 1000;
            NativeList<InstanceData> instanceDataArray = new NativeList<InstanceData>(
                initialSize,
                Allocator.Persistent
            );

            for (int d = 0; d < 6; d++)
            {
                for (int crossPos0 = 0; crossPos0 < 32; crossPos0++)
                {

                    int crossPos1 = 0;
                    int index = d + 6 * crossPos0 + 6 * 32 * crossPos1;
                    uint stack = swizzledStructure[index];

                    while (true)
                    {
                        int2 range = FindStartAndEnd(stack);
                        uint mask = GenerateRangeMask(range);
                        stack = ClearBits(stack, mask);

                        int endRow = crossPos1 + 1;
                        while (endRow < 32)
                        {
                            int endRowIndex = d + 6 * crossPos0 + 6 * 32 * endRow;
                            uint endRowStack = swizzledStructure[endRowIndex];

                            if ((endRowStack | mask) == endRowStack)
                            {
                                endRow++;
                                swizzledStructure[endRowIndex] = ClearBits(endRowStack, mask);
                            }
                            else
                            {
                                break;
                            }
                        }

                        // append quad
                        if (mask != 0u)
                        {

                            uint3 standardPos = SwitchToStandardRepresentationUnsigned(
                                (uint)range.x,
                                (uint)crossPos0,
                                (uint)crossPos1,
                                d
                            );

                            uint3 standardScale = SwitchToStandardRepresentationUnsigned(
                                (uint)range.y - (uint)range.x - 1u,
                                0u,
                                (uint)endRow - (uint)crossPos1 - 1u,
                                d
                            );

                            // UnityEngine.Debug.Log(
                            //     $"xScale {standardScale.x}, yScale {standardScale.y}, zScale {standardScale.z}");

                            uint encoded = EncodeQuad(new QuadData
                            {
                                x = standardPos.x,
                                y = standardPos.y,
                                z = standardPos.z,
                                xScale = standardScale.x,
                                yScale = standardScale.y,
                                zScale = standardScale.z
                            });
                            instanceDataArray.Add(new InstanceData
                            {
                                EncodedQuadData = encoded,
                                Direction = (uint)d
                            });

                        }

                        // move on to next row
                        if (stack == 0u)
                        {
                            crossPos1++;
                            if (crossPos1 < 32)
                            {
                                index = d + 6 * crossPos0 + 6 * 32 * crossPos1;
                                stack = swizzledStructure[index]; // update to next row stack
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
            }
            return instanceDataArray;
        }

        public static NativeList<InstanceData> GreedyQuadsPipeline(NativeArray<uint> voxels)
        {
            // structuring
            NativeArray<uint> structure = CreateStructure();
            FillStructureWithoutJob(voxels, structure);

            // swizzling
            NativeArray<uint> swizzledStructure = CreateStructure();
            FillSwizzledStructureWithoutJob(structure, swizzledStructure);

            NativeList<InstanceData> instanceDataArray = GenerateGreedyQuads(swizzledStructure);

            structure.Dispose();
            swizzledStructure.Dispose();

            return instanceDataArray;
        }
        
        public static NativeList<InstanceData> GreedyQuadsPipeline(NativeSlice<uint> voxels)
        {
            // structuring
            NativeArray<uint> structure = CreateStructure();
            FillStructureWithoutJob(voxels, structure);

            // swizzling
            NativeArray<uint> swizzledStructure = CreateStructure();
            FillSwizzledStructureWithoutJob(structure, swizzledStructure);

            NativeList<InstanceData> instanceDataArray = GenerateGreedyQuads(swizzledStructure);

            structure.Dispose();
            swizzledStructure.Dispose();

            return instanceDataArray;
        }
    }
}