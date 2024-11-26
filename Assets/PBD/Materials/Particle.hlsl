// Make sure file is not included twice
#ifndef PARTICLE_HLSL
#define PARTICLE_HLSL

// Includes
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderVariablesFunctions.hlsl"

// Includes to enable indirect draw
#define UNITY_INDIRECT_DRAW_ARGS IndirectDrawIndexedArgs
#include "UnityIndirect.cginc"

// Scale and center of the simulation
float _Scale;
float _Damping;
float3 _SimulationCenter;

// Particle data
float _ParticleSize;
StructuredBuffer<float4> _Particles;

// Fragment shader input data
struct VertexOutput {
    float3 positionWS : TEXCOORD1;
    float4 positionCS : SV_POSITION;
    float3 normalWS : TEXCOORD2;
};

// Mesh vertex input data
struct InstancedVertexInput {
    float3 position : POSITION;
    float3 normal : NORMAL;
    float2 uv : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

// Vertex shader
VertexOutput vert(InstancedVertexInput input, uint svInstanceID : SV_InstanceID) {
    VertexOutput output = (VertexOutput)0;

    // Get instance data
    InitIndirectDrawArgs(0);
    uint cmdID = GetCommandID(0);
    uint instanceID = GetIndirectInstanceID(svInstanceID);
    float4 particle = _Particles[instanceID];

    // Output combined data
    output.positionWS = ((input.position * _ParticleSize + particle.xyz)) * _Scale + _SimulationCenter * _Damping;
    output.positionCS = TransformWorldToHClip(output.positionWS);
    output.normalWS = input.normal;

    return output;
}

// Shader properties
half4 _Color;

// Fragment shader
half4 frag(VertexOutput input, uint svInstanceID : SV_InstanceID) : SV_Target {

    // Gather data for lighting
    InputData lightingData = (InputData)0;
    lightingData.positionWS = input.positionWS;
    lightingData.normalWS = input.normalWS;
    lightingData.viewDirectionWS = GetWorldSpaceViewDir(lightingData.positionWS);
    lightingData.shadowCoord = TransformWorldToShadowCoord(input.positionWS);

    // Gather data for surface
    SurfaceData surfaceData = (SurfaceData)0;
    surfaceData.albedo = _Color.xyz;
    surfaceData.alpha = _Color.a;

    // Use URP's Blinn-Phong lighting model (Bloom combined with MSAA and HDR causes flickering specular highlights)
    half4 lighting = UniversalFragmentBlinnPhong(lightingData, surfaceData);
    half4 ambient = half4(_GlossyEnvironmentColor.xyz * _Color.xyz, 1);
    return lighting + ambient;
}

#endif // PARTICLE_HLSL