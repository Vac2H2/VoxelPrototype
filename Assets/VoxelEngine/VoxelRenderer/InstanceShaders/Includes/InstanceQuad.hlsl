//UNITY_SHADER_NO_UPGRADE
#ifndef MYHLSLINCLUDE_INCLUDED
#define MYHLSLINCLUDE_INCLUDED

#include "QuadInstances.hlsl"

struct QuadData
{       
    uint x;
    uint y;
    uint z;
    uint xScale;
    uint yScale;
    uint zScale;
};

struct CompactInstanceData
{
    QuadData PositionAndScale;
    float Direction;
    float3 ChunkPosition;
    float TypeIndex;
};

StructuredBuffer<CompactInstanceData> _validInstanceBuffer;
StructuredBuffer<uint> _typeBuffer;
float3 _scale;

uint2 GetProjectedCoord(uint3 coord, uint direction)
{
    uint group = direction >> 1;  

    int eq0 = (((int)group ^ 0) - 1) >> 31 & 1; 
    int eq1 = (((int)group ^ 1) - 1) >> 31 & 1; 

    uint firstIndex  = 2 * eq0;     
    uint secondIndex = 1 + eq1;     

    return uint2(coord[firstIndex], coord[secondIndex]);
}

void GetPositionNormalUV_float(
    float instanceID, 
    float vertexID, 
    out float3 position, 
    out float3 normal, 
    out float2 uv,
    out float3 localPosition,
    out float typeIndex)
{
    CompactInstanceData instanceData = _validInstanceBuffer[instanceID];
    typeIndex = instanceData.TypeIndex + 0.5;
    int3 worldPosition = instanceData.ChunkPosition;

    QuadData quadData = instanceData.PositionAndScale;

    uint direction = instanceData.Direction;

    float3 insPos = MESH_VERTS[direction][vertexID];
    float3 insNormal = MESH_NORMALS[direction];
    float2 insUV  = MESH_UVS[direction][vertexID];

    float3 quadScales = float3(
        (float)quadData.xScale,
        (float)quadData.yScale,
        (float)quadData.zScale
    );

    float3 scaledPosition = insPos * quadScales;

    float3 chunkOffset = float3(
        (float)quadData.x, 
        (float)quadData.y, 
        (float)quadData.z
    );
    
    float3 positionBeforeTransform = scaledPosition + chunkOffset + worldPosition * 32;

    position = positionBeforeTransform * _scale;

    normal = insNormal;

    float2 wh = (float2)GetProjectedCoord((uint3)scales, direction);
    uv = insUV * wh;
    
    localPosition = scaledPosition + chunkOffset;
    localPosition = lerp(localPosition, chunkOffset + 0.5, abs(insNormal));
}

void GetVoxelType_float(float typeIndex, float3 localPosition, out float type)
{
    int3 pos = floor(localPosition);
    type = (float)_typeBuffer[floor(typeIndex) * 32 * 32 * 32 + pos.x + 32 * pos.y + 32 * 32 * pos.z];
}

#endif //MYHLSLINCLUDE_INCLUDED
