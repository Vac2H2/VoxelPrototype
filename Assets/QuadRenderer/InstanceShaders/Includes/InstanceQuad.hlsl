//UNITY_SHADER_NO_UPGRADE
#ifndef MYHLSLINCLUDE_INCLUDED
#define MYHLSLINCLUDE_INCLUDED

#include "QuadInstances.hlsl"

struct InstanceData
{
    uint EncodedQuad;
    uint Direction;
};

struct QuadData
{       
    uint x;
    uint y;
    uint z;
    uint xScale;
    uint yScale;
    uint zScale;
};

StructuredBuffer<InstanceData> _instanceBuffer;
StructuredBuffer<int> _instanceCountBuffer;
StructuredBuffer<int3> _chunkPositionBuffer;
StructuredBuffer<int> _typeIndexBuffer;
StructuredBuffer<uint> _typeBuffer;

uint2 GetProjectedCoord(uint3 coord, uint direction)
{
    uint group = direction >> 1;  

    int eq0 = (((int)group ^ 0) - 1) >> 31 & 1; 
    int eq1 = (((int)group ^ 1) - 1) >> 31 & 1; 

    uint firstIndex  = 2 * eq0;     
    uint secondIndex = 1 + eq1;     

    return uint2(coord[firstIndex], coord[secondIndex]);
}

QuadData DecodeQuad(uint data)
{
    QuadData result;
    result.x       =  (data >> 0)  & 0x1F;  
    result.y       =  (data >> 5)  & 0x1F;  
    result.z       =  (data >> 10) & 0x1F;  
    result.xScale  =  ((data >> 15) & 0x1F) + 1;  
    result.yScale  =  ((data >> 20) & 0x1F) + 1;  
    result.zScale  =  ((data >> 25) & 0x1F) + 1;
    return result;
}

void GetPositionNormalUV_float(
    float instanceID, 
    float vertexID, 
    float4x4 transformationMatrix,
    out float3 position, 
    out float3 normal, 
    out float2 uv,
    out float3 localPosition,
    out float typeIndex)
{
    uint blockID = (uint)instanceID / 1024;

    uint instanceCount = _instanceCountBuffer[blockID];
    if ((uint)instanceID >= 1024 * blockID + instanceCount)
    {
        return;
    }


    InstanceData instanceData = _instanceBuffer[instanceID];
    typeIndex = _typeIndexBuffer[blockID];
    int3 worldPosition = _chunkPositionBuffer[blockID];

    QuadData quadData = DecodeQuad(instanceData.EncodedQuad);

    uint direction = instanceData.Direction;

    float3 insPos = MESH_VERTS[direction][vertexID];
    float3 insNormal = MESH_NORMALS[direction];
    float2 insUV  = MESH_UVS[direction][vertexID];

    float3 scales = float3(
        (float)quadData.xScale,
        (float)quadData.yScale,
        (float)quadData.zScale
    );

    float3 scaledPosition = insPos * scales;

    float3 chunkOffset = float3(
        (float)quadData.x, 
        (float)quadData.y, 
        (float)quadData.z
    );
    
    float3 positionBeforeTransform = scaledPosition + chunkOffset + worldPosition;

    position 
    = mul(transformationMatrix, positionBeforeTransform) 
    + float3(transformationMatrix[0][3], transformationMatrix[1][3], transformationMatrix[2][3]);

    normal = mul(transformationMatrix, insNormal);

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
