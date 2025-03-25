using Unity.Collections;
using Unity.Mathematics;

namespace VoxelEngine.VoxelManager
{
	public static partial class VoxelInstanceGeneration
	{
		public static NativeList<VoxelInstanceManager.InstanceData> InstancePipeline(NativeSlice<uint> voxels)
        {
            // structuring
            NativeArray<uint> structure = CreateStructure();
            FillStructure(voxels, structure);

            // swizzling
            NativeArray<uint> swizzledStructure = CreateStructure();
            FillSwizzledStructure(structure, swizzledStructure);

            NativeList<VoxelInstanceManager.InstanceData> instances
			= GenerateGreedyQuads(swizzledStructure);

            structure.Dispose();
            swizzledStructure.Dispose();

            return instances;
        }
	}
}