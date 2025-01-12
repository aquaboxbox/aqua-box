struct VertexInput
{
    float4 model_pos : POSITION;
    float2 uv: TEXCOORD0;
    uint id: SV_VertexID;

    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct VertexOutput
{
    float4 clipPos : SV_POSITION;
    float2 uv: TEXCOORD0;
    float3 worldPos : W_POSITION;

    UNITY_VERTEX_INPUT_INSTANCE_ID
};

StructuredBuffer<float4> _PositionBuffer;
float _Radius;
float _Scale;
float _Damping;
float3 _SimulationCenter;

VertexOutput BillboardVert(VertexInput vIn, uint instanceID : SV_InstanceID)
{
    VertexOutput vOut;

    UNITY_SETUP_INSTANCE_ID(vIn);
    UNITY_TRANSFER_INSTANCE_ID(vIn, vOut);

    // Generate model pos
    float3 center = _PositionBuffer[instanceID] * _Scale + _SimulationCenter * _Damping;;
    float3 cameraPos = _WorldSpaceCameraPos;

    float3 cameraDir = normalize(cameraPos - center);
    float3 upDir = normalize(float3(0.0, 1.0, 0.0));
    float3 v = normalize(cross(cameraDir, upDir));
    float3 u = normalize(cross(v, cameraDir));

    float3 worldPos = center;
    int id = vIn.id;
    if (id == 0) {
        worldPos -= u * _Radius;
        worldPos -= v * _Radius;
    } else if (id == 1) {
        worldPos -= u * _Radius;
        worldPos += v * _Radius;
    } else if (id == 2) {
        worldPos += u * _Radius;
        worldPos -= v * _Radius;
    } else if (id == 3) {
        worldPos += u * _Radius;
        worldPos += v * _Radius;
    }

    vOut.clipPos = UnityObjectToClipPos(float4(worldPos, 1));
    vOut.worldPos = worldPos;
    vOut.uv = vIn.uv;

    return vOut;
}

