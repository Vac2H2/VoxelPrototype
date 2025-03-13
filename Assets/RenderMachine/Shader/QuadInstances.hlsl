static const float3 MESH_VERTS[6][4] =
{
    // Mesh #0
    {
        float3(1, 0, 0),
        float3(1, 1, 0),
        float3(1, 1, 1),
        float3(1, 0, 1),
    },
    // Mesh #1
    {
        float3(0, 0, 1),
        float3(0, 1, 1),
        float3(0, 1, 0),
        float3(0, 0, 0),
    },
    // Mesh #2
    {
        float3(0, 1, 0),
        float3(0, 1, 1),
        float3(1, 1, 1),
        float3(1, 1, 0),
    },
    // Mesh #3
    {
        float3(1, 0, 0),
        float3(1, 0, 1),
        float3(0, 0, 1),
        float3(0, 0, 0),
    },
    // Mesh #4
    {
        float3(1, 0, 1),
        float3(1, 1, 1),
        float3(0, 1, 1),
        float3(0, 0, 1),
    },
    // Mesh #5
    {
        float3(0, 0, 0),
        float3(0, 1, 0),
        float3(1, 1, 0),
        float3(1, 0, 0),
    }
};

// For each mesh, define 4 UV coordinates in the same layout.
static const float2 MESH_UVS[6][4] =
{
    // Mesh #0
    {
        float2(0,0),
        float2(0,1),
        float2(1,1),
        float2(1,0),
    },
    // Mesh #1
    {
        float2(1,0),
        float2(1,1),
        float2(0,1),
        float2(0,0),
    },
    // Mesh #2
    {
        float2(0,0),
        float2(0,1),
        float2(1,1),
        float2(1,0),
    },
    // Mesh #3
    {
        float2(1,0),
        float2(1,1),
        float2(0,1),
        float2(0,0),
    },
    // Mesh #4
    {
        float2(1,0),
        float2(1,1),
        float2(0,1),
        float2(0,0),
    },
    // Mesh #5
    {
        float2(0,0),
        float2(0,1),
        float2(1,1),
        float2(1,0),
    },
};

static const float3 MESH_NORMALS[6] = 
{
    float3(1, 0, 0),
    float3(-1, 0, 0),
    float3(0, 1, 0),
    float3(0, -1, 0),
    float3(0, 0, 1),
    float3(0, 0, -1)
};
