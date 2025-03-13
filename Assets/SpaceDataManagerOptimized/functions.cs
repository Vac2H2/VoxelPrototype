using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public partial class SpaceDataManagerOptimized
{
    public void AddChunk(int3 chunkPosition, NativeArray<uint> state, NativeArray<uint> type)
    {
        if (chunkMap.TryAdd(chunkPosition, numChunkAdded))
        {
            int3[] position = new int3[1];
            position[0] = chunkPosition * new int3(32, 32, 32); // actual chunk position
            if (numChunkAdded + 1 > chunkPositionBuffer.count)
            {
                ComputeBuffer positionBufferExpanded = GetExpandedBuffer(chunkPositionBuffer, sizeof(int) * 3, 64);
                chunkPositionBuffer.Release();
                chunkPositionBuffer = positionBufferExpanded;
            }
            chunkPositionBuffer.SetData(position, 0, numChunkAdded, 1);

            stateList.AddRange(state);
            typeList.AddRange(type);

            if ((numChunkAdded + 1) * TYPE_SIZE > typeBuffer.count)
            {
                ComputeBuffer typeBufferExpanded = GetExpandedBuffer(typeBuffer, sizeof(uint), 64 * TYPE_SIZE);
                typeBuffer.Release();
                typeBuffer = typeBufferExpanded;
            }
            typeBuffer.SetData(type, 0, numChunkAdded * TYPE_SIZE, TYPE_SIZE);

            // set new chunk dirty
            dirtyChunks.Enqueue(chunkPosition);

            numChunkAdded++;
        }
    }

    public void SetChunkDirty(int3 position)
    {
        if (chunkMap.TryGetValue(position, out int chunkIndex))
        {
            dirtyChunks.Enqueue(position);
        }
    }

    public NativeQueue<int3> GetDirtyChunks()
    {
        return dirtyChunks;
    }

    public bool GetChunkStateSlice(int3 position, out NativeSlice<uint> slice)
    {
        if (chunkMap.TryGetValue(position, out int chunkIndex))
        {
            slice = new NativeSlice<uint>(stateList.AsArray(), chunkIndex * STATE_SIZE, STATE_SIZE);
            return true;
        }
        else
        {
            slice = default;
            return false;
        }
    }

    public bool GetChunkTypeSlice(int3 position, out NativeSlice<uint> slice)
    {
        if (chunkMap.TryGetValue(position, out int chunkIndex))
        {
            slice = new NativeSlice<uint>(typeList.AsArray(), chunkIndex * TYPE_SIZE, TYPE_SIZE);
            return true;
        }
        else
        {
            slice = default;
            return false;
        }
    }

    public int GetInstanceBufferCount()
    {
        return instanceBuffer.count;
    }

    public void UpdateTransformationMatrix(Matrix4x4 trs)
    {
        transformationMatrix = trs;
    }

    public Matrix4x4 GetTransformationMatrix()
    {
        return transformationMatrix;
    }
}