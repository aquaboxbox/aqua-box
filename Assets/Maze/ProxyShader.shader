Shader "Unlit/ProxyShader"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

			HLSLPROGRAM

			// Signal this shader requires a compute buffer
			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x
			#pragma target 5.0

			// GPU Instancing
			#pragma multi_compile_instancing
			#pragma instancing_options assumeuniformscaling nomatrices nolightprobe nolightmap

			// Lighting and shadow keywords
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
			#pragma multi_compile _ _ADDITIONAL_LIGHTS
			#pragma multi_compile _ _ADDITIONAL_LIGHTS_SHADOWS
			#pragma multi_compile _ _SHADOWS_SOFT
			#pragma multi_compile _ _SHADOWS_CASCADE
            
            // Register functions
			#pragma vertex vert
			#pragma fragment frag

            // Includes
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // Includes to enable indirect draw
            #define UNITY_INDIRECT_DRAW_ARGS IndirectDrawIndexedArgs
            #include "UnityIndirect.cginc"

            // Target position to render proxy around
			float3 _ProxyTarget;
            float _ProxyRadius;
            half4 _Color;

            // Mesh vertex input data
            struct InstancedVertexInput {
                float3 position : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            // Fragment shader input data
            struct VertexOutput {
                float3 positionWS : TEXCOORD1;
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD2;
            };

            // Vertex shader
            VertexOutput vert(InstancedVertexInput input, uint svInstanceID : SV_InstanceID) {
                VertexOutput output = (VertexOutput)0;

                // Get instance data
                InitIndirectDrawArgs(0);
                uint cmdID = GetCommandID(0);
                uint instanceID = GetIndirectInstanceID(svInstanceID);

                // Output combined data
                output.positionWS = mul(unity_ObjectToWorld, float4(input.position, 1)).xyz;
                output.positionCS = TransformWorldToHClip(output.positionWS);
                output.normalWS = input.normal;

                return output;
            }

            // Fragment shader
            half4 frag(VertexOutput input, uint svInstanceID : SV_InstanceID) : SV_Target {

                // Set Opacity based on distance from target
                float3 target = _ProxyTarget;
                float3 viewDir = GetWorldSpaceViewDir(input.positionWS);
                float3 dir = target - input.positionWS;
                float dist = length(dir);
                float opacity = saturate(1 - dist / _ProxyRadius);

                // Discard if outside radius
                if (dist > _ProxyRadius) {
                    discard;
                }

                // Screen space dithering
                float ditheringOpacity = 1 - (pow(1 - saturate(1 - dist / _ProxyRadius), 1.25f));
                float4 positionCS = TransformWorldToHClip(input.positionWS);
                if (frac(positionCS.x * _ScreenParams.x) > ditheringOpacity && frac(positionCS.y * _ScreenParams.y) > ditheringOpacity) {
                    discard;
                }

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
                half4 ambient = half4(_GlossyEnvironmentColor.xyz * _Color.xyz, _Color.a);
                return (lighting + ambient) * opacity;
            }

            ENDHLSL
        }
    }
}
