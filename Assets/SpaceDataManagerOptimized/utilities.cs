using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

public partial class SpaceDataManagerOptimized
{
    public ComputeBuffer GetExpandedBuffer(
        ComputeBuffer sourceBuffer,
        int stride,
        int size)
    {
        int oldSize = sourceBuffer.count;
        ComputeBuffer expandedBuffer = new ComputeBuffer(
            oldSize + size,
            stride,
            ComputeBufferType.Structured
        );

        // copy old data
        int kernelID = copyShader.FindKernel("CopyInstanceDataBuffer");
        copyShader.SetBuffer(kernelID, "_sourceBuffer", sourceBuffer);
        copyShader.SetBuffer(kernelID, "_targetBuffer", expandedBuffer);
        copyShader.SetInt("_offset", 0);
        copyShader.SetInt("_actualLength", oldSize);
        int threadGroupCount = Mathf.CeilToInt(oldSize / 256.0f);
        copyShader.Dispatch(kernelID, threadGroupCount, 1, 1);

        return expandedBuffer;
    }

    public struct PackedData
    {
        public NativeHashMap<int3, int> ChunkMap;
        public NativeHashMap<int, NativeList<int>> BlockIndexMap;
        public NativeQueue<int> AvailableBlockIndex;
        public ComputeBuffer InstanceBuffer;
        public ComputeBuffer ChunkIndexBuffer;
        public ComputeBuffer ChunkPositionBuffer;
        public NativeList<uint> StateList;
        public NativeList<uint> TypeList;
        public ComputeBuffer TypeBuffer;
        public ComputeBuffer InstanceCountBuffer;
    }

    public PackedData GetPackedData()
    {
        return new PackedData
        {
            ChunkMap = chunkMap,
            BlockIndexMap = blockIndexMap,
            AvailableBlockIndex = availableBlockIndex,
            InstanceBuffer = instanceBuffer,
            ChunkIndexBuffer = chunkIndexBuffer,
            ChunkPositionBuffer = chunkPositionBuffer,
            StateList = stateList,
            TypeList = typeList,
            TypeBuffer = typeBuffer,
            InstanceCountBuffer = instanceCountBuffer
        };
    }
}