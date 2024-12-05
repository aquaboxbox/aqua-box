Shader "Custom/WaterShader"
{
    Properties { }

    SubShader {
        Tags { 
            "RenderPipeline" = "UniversalPipeline" 
            "RenderType" = "Transparent" 
            "Queue" = "Transparent"
        }

        // depth
        Pass
        {
            ZWrite On
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"

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

            float _Radius;
            float _DepthRounding;
            StructuredBuffer<float4> _PositionBuffer;

            VertexOutput vert(VertexInput vIn, uint instanceID : SV_InstanceID)
            {
                VertexOutput vOut;

                UNITY_SETUP_INSTANCE_ID(vIn);
                UNITY_TRANSFER_INSTANCE_ID(vIn, vOut);

                // Generate model pos
                float3 center = _PositionBuffer[instanceID];
                float3 cameraPos = _WorldSpaceCameraPos;

                float3 cameraDir = normalize(cameraPos - center);
                float3 upDir = normalize(float3(0.0, 1.0, 0.0));
                float3 v = normalize(cross(cameraDir, upDir));
                float3 u = normalize(cross(v, cameraDir));

                float3 worldPos = center;
                //worldPos = center;
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


            float frag(VertexOutput vIn): SV_Depth
            {
                UNITY_SETUP_INSTANCE_ID(vIn);

                float2 normUv = vIn.uv * 2.0 - 1.0;
                float dist = length(normUv);

                bool inMask = dist >= 1.0;
                if (inMask) {
                    discard;
                }
 
                // must be recalculated since interpolating clipPos does not work
                // protrude depth towards camera based of distance to center
                float3 worldPos = vIn.worldPos;
                float3 cameraDir = normalize(_WorldSpaceCameraPos - worldPos);
                float3 worldFragPos = worldPos + cameraDir * (1.0 - dist * dist) * _DepthRounding;

                float4 clipPos = UnityObjectToClipPos(worldFragPos);
                float depth = clipPos.z / clipPos.w;

                return depth;

            }
            ENDCG
        }  

        // thickness
        Pass {
            ZWrite Off
            ZTest On
            Blend SrcAlpha OneMinusSrcAlpha
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"

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

            float _Radius;
            float _Thickness;
            StructuredBuffer<float4> _PositionBuffer;

            VertexOutput vert(VertexInput vIn, uint instanceID : SV_InstanceID)
            {
                VertexOutput vOut;

                UNITY_SETUP_INSTANCE_ID(vIn);
                UNITY_TRANSFER_INSTANCE_ID(vIn, vOut);

                // Generate model pos
                float3 center = _PositionBuffer[instanceID];
                float3 cameraPos = _WorldSpaceCameraPos;

                float3 cameraDir = normalize(cameraPos - center);
                float3 upDir = normalize(float3(0.0, 1.0, 0.0));
                float3 v = normalize(cross(cameraDir, upDir));
                float3 u = normalize(cross(v, cameraDir));

                float3 worldPos = center;
                //worldPos = center;
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


            float4 frag(VertexOutput vIn): SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(vIn);

                float2 normUv = vIn.uv * 2.0 - 1.0;
                float dist = length(normUv);

                // SET BLEND MODE IN TAGS

                bool inMask = dist >= 1.0;
                if (inMask) {
                    discard;
                }

                float thickness = (1.0 - dist * dist) * _Thickness;

                return float4(1,1,1,thickness);

            }
            ENDCG

        }

        // bilateral blur
        // depth blur
        Pass {
            ZWrite On
            ZTest Off

            HLSLPROGRAM

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            #pragma vertex Vert     // take vert from blit package
            #pragma fragment frag

            sampler2D _DepthTexture;
            int _BlurRadius;
            float _IntensitySigma;
            float _DistanceSigma;

            // x: distance from x0
            float gaussian1(float x, float sigma) {
                return exp(-(x * x) / (2.0 * sigma * sigma));
            }

            // x: distance from x0
            // y: distance from y0
            float gaussian2(float x, float y, float sigma) {
                return exp(-(x * x + y * y) / (2.0 * sigma * sigma));
            }

            float frag(Varyings input) : SV_Depth
            {
                float2 uv = input.texcoord.xy;
                float2 texelSize = 1.0 / float2(_ScreenParams.x, _ScreenParams.y);
                float depth = tex2D(_DepthTexture, uv).r;
                //int kernelSize = (int)((float)_BlurRadius * (1.0 - depth));
                int kernelSize = _BlurRadius;

                float sum = 0.0;
                float weightSum = 0.0;
                //[loop]
                for (int i = -kernelSize; i <= kernelSize; i++) {
                    //[loop]
                    for (int j = -kernelSize; j <= kernelSize; j++) {
                        float2 sampleUv = uv + float2(i,j) * texelSize;

                        float sampleDepth = tex2D(_DepthTexture, sampleUv).r;
                        float spatial = gaussian1(length(float2(i,j)), _DistanceSigma);
                        float intensity = gaussian1(abs(depth - sampleDepth), _IntensitySigma);
                        float weight = spatial * intensity;
                        //float weight = gaussian2(i, j, _DistanceSigma);// * gaussian1(abs(depth - sampleDepth), _IntensitySigma);

                        sum += sampleDepth * weight;
                        weightSum += weight;
                    }
                }
                sum /= weightSum;

                return sum;
            }
            ENDHLSL
        }
        // gaussian blur
        // color
        Pass {
            ZWrite Off
            ZTest Off

            HLSLPROGRAM

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            #pragma vertex Vert     // take vert from blit package
            #pragma fragment frag

            int _ThicknessBlurRadius;
            float _ThicknessSigma;

            // x: distance from x0
            float gaussian1(float x, float sigma) {
                return exp(-(x * x) / (2.0 * sigma * sigma));
            }

            float4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord.xy;
                float2 texelSize = 1.0 / float2(_ScreenParams.x, _ScreenParams.y);
                float4 color = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_PointClamp, uv, _BlitMipLevel);
                int kernelSize = _ThicknessBlurRadius;

                float4 sum = float4(0,0,0,0);
                float weightSum = 0.0;
                for (int i = -kernelSize; i <= kernelSize; i++) {
                    for (int j = -kernelSize; j <= kernelSize; j++) {
                        float2 sampleUv = uv + float2(i,j) * texelSize;

                        float4 sampleColor = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_PointClamp, sampleUv, _BlitMipLevel);
                        float weight = gaussian1(length(float2(i,j)), _ThicknessSigma);

                        sum += sampleColor * weight;
                        weightSum += weight;
                    }
                }
                sum /= weightSum;

                return sum;
            }
            ENDHLSL
        }
        // depth blit
        Pass {
            ZWrite On
            ZTest Off

            HLSLPROGRAM

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            
            sampler2D _CameraDepthTexture;
            sampler2D _HalfResDepthTexture;

            #pragma vertex Vert
            #pragma fragment frag

            float frag(Varyings input) : SV_Depth
            {
                float4 sceneColor = FragBilinear(input);
                //return sceneColor;
                float d1 = tex2D(_CameraDepthTexture, input.texcoord).r;
                float d2 = tex2D(_HalfResDepthTexture, input.texcoord).r;
                float d = max(d1, d2);

                return d;
            }
            ENDHLSL
        }

        // render
        Pass {
            ZWrite off
                
            HLSLPROGRAM

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            sampler2D _CameraDepthTexture;
            sampler2D _DepthTexture;
            sampler2D _ThicknessTexture;
            float4x4 _InverseView;
            float4x4 _InverseProjection;
            float4x4 _InverseViewProjection;
            float3 _LightPos;

            #pragma vertex Vert     // take vert from blit package
            #pragma fragment frag

            float4 blend(float4 sceneColor, float4 color, float alpha) 
            {
                return (1 - alpha) * sceneColor + alpha * color;
            }

            float fresnel(float cosTheta)
            {
                return pow(saturate(1.0 - cosTheta), 15.0);
                return pow(saturate(1.0 - cosTheta), 5.0);
            }

            float3 reconstructPosition(float2 uv, float depth)
            {
            #if defined(UNITY_REVERSED_Z)
                depth = 1.0f - depth;
            #endif
                float x = uv.x * 2.0 - 1.0;
                float y = uv.y * 2.0 - 1.0;
                float z = depth * 2.0 - 1.0;
                    
                float4 position_s = float4(x, y, z, 1.0f);
                float4 position_v = mul(_InverseViewProjection, position_s);
                return position_v.xyz / position_v.w;
            }

            float4 frag(Varyings input) : SV_Target
            {
                float4 sceneColor = FragBilinear(input);
                float2 uv = input.texcoord;

                float2 texelSize = 1.0 / float2(_ScreenParams.x, _ScreenParams.y);
                // https://wickedengine.net/2019/09/improved-normal-reconstruction-from-depth/
                float centerDepth = tex2D(_DepthTexture, uv + float2(0, 0) * texelSize).r;
                float leftDepth   = tex2D(_DepthTexture, uv + float2(-1,0) * texelSize).r;
                float rightDepth  = tex2D(_DepthTexture, uv + float2(1, 0) * texelSize).r;
                float topDepth    = tex2D(_DepthTexture, uv + float2(0,-1) * texelSize).r;
                float bottomDepth = tex2D(_DepthTexture, uv + float2(0, 1) * texelSize).r;

                float3 centerPos = reconstructPosition(uv + float2(0, 0) * texelSize, centerDepth);
                float3 leftPos   = reconstructPosition(uv + float2(-1,0) * texelSize, leftDepth);
                float3 rightPos  = reconstructPosition(uv + float2(1, 0) * texelSize, rightDepth);
                float3 topPos    = reconstructPosition(uv + float2(0,-1) * texelSize, topDepth);
                float3 bottomPos = reconstructPosition(uv + float2(0, 1) * texelSize, bottomDepth);

                if (centerDepth <= 0.01) {
                    return sceneColor;
                }

                bool leftBest = abs(leftDepth - centerDepth) < abs(rightDepth - centerDepth);
                bool topBest = abs(topDepth - centerDepth) < abs(bottomDepth - centerDepth);

                float3 P0 = centerPos;
                float3 P1;
                float3 P2;
                if (leftBest  && topBest) { P1 = leftPos; P2 = topPos; }         // top left
                if (!leftBest && topBest) { P1 = topPos; P2 = rightPos; }       // top right
                if (leftBest  && !topBest) { P1 = bottomPos; P2 = leftPos; }     // bottom left
                if (!leftBest && !topBest) { P1 = rightPos; P2 = bottomPos; }   // bottom right

                float3 normal = normalize(cross(P2 - P0, P1 - P0));

                float3 lightPos = _LightPos;
                float3 lightDir = normalize(lightPos - centerPos);
                float3 viewDir = normalize(_WorldSpaceCameraPos - centerPos);
                float3 halfDir = normalize(lightDir + viewDir);

                float ambient  = 0.1;
                float diffuse  = 0.8 * saturate(dot(lightDir, normal));
                float specular = 0.5 * pow(saturate(dot(halfDir, normal)), 150.0);

                //return float4(1,1,1,1) * centerDepth;
                //return float4(normal,1);
                //return float4(1,1,1,1) * clamp(fresnel(dot(normal, viewDir)), 0.0, 0.6) * 0.3;

                float thickness = tex2D(_ThicknessTexture, uv).r;
                //return float4(thickness,thickness,thickness,1);
                //return float4(centerDepth,centerDepth,centerDepth,1);

                float3 fluidColor = float3(0.0, 0.4, 0.6);
                float3 refractedColor = lerp(fluidColor, sceneColor.xyz, exp(-thickness));
                //float3 refractedColor = lerp(fluidColor, sceneColor.xyz, exp(-thickness));
                float3 reflectedColor = float3(1,1,1);
                float fresnelFactor = fresnel(dot(normal, viewDir));
                //return float4(fresnelFactor, fresnelFactor, fresnelFactor, 1);
                float3 col = refractedColor * (1.0 - fresnel(dot(normal, viewDir))) + reflectedColor * fresnel(dot(normal, viewDir)) + specular;
                //return float4(fluidColor, 1);
                //return float4(1,1,1, 1);
                return float4(col, 1);

                float light = ambient + diffuse + specular;
                //float light = specular;
                float3 color = sceneColor;
                if (tex2D(_CameraDepthTexture, uv).r < tex2D(_DepthTexture, uv).r) {
                    color = float3(0.0, 0.4, 0.6) * light;
                }

                //return sceneColor;
                return float4(color, 1);
                return float4(centerPos, 1);
            }
            ENDHLSL
        }
        // clear depth
        Pass {
            ZWrite On 
            ZTest Off

            HLSLPROGRAM

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            
            #pragma vertex Vert
            #pragma fragment frag

            float frag(Varyings input) : SV_Depth
            {
                return 0.0;
            }
            ENDHLSL
        }
        //// clear color
        //Pass {
        //    ZWrite Off
        //    ZTest Off

        //    HLSLPROGRAM

        //    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        //    #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
        //    
        //    #pragma vertex Vert
        //    #pragma fragment frag

        //    float4 frag(Varyings input) : SV_Target
        //    {
        //        return float4(0.0, 0.0, 0.0, 1.0);
        //    }
        //    ENDHLSL
        //}
        // color blit
       // Pass {
       //     ZWrite Off
       //     ZTest Off

       //     HLSLPROGRAM

       //     #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
       //     #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
       //     
       //     sampler2D _CameraDepthTexture;
       //     sampler2D _TempDepthTexture;

       //     #pragma vertex Vert
       //     #pragma fragment frag

       //     float frag(Varyings input) : SV_Target
       //     {
       //         float4 sceneColor = FragBilinear(input);
       //         //return sceneColor;
       //         float d1 = tex2D(_CameraDepthTexture, input.texcoord).r;
       //         float d2 = tex2D(_TempDepthTexture, input.texcoord).r;
       //         float d = max(d1, d2);

       //         return d;
       //     }
       //     ENDHLSL
       // }

       // //// horizontal blur
        //Pass {
        //    ZWrite On

        //    HLSLPROGRAM

        //    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        //    #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

        //    #pragma vertex Vert     // take vert from blit package
        //    #pragma fragment frag

        //    sampler2D _CameraDepthTexture;
        //    int _BlurRadius;

        //    float frag(Varyings input) : SV_Depth
        //    {
        //        float2 uv = input.texcoord.xy;
        //        float2 texelSize = 1.0 / float2(_ScreenParams.x, _ScreenParams.y);

        //        float depth = 0.0;
        //        for (int i = -_BlurRadius; i <= _BlurRadius; i++) {
        //            depth += tex2D(_CameraDepthTexture, uv + float2(i, 0) * texelSize).r;
        //        }
        //        depth /= (_BlurRadius * 2 + 1);

        //        return depth;
        //    }
        //    ENDHLSL
        //}
        //// vertical blur
        //Pass {
        //    ZWrite On

        //    HLSLPROGRAM

        //    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        //    #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

        //    #pragma vertex Vert     // take vert from blit package
        //    #pragma fragment frag

        //    sampler2D _CameraDepthTexture;
        //    int _BlurRadius;

        //    float frag(Varyings input) : SV_Depth
        //    {
        //        float2 uv = input.texcoord.xy;
        //        float2 texelSize = 1.0 / float2(_ScreenParams.x, _ScreenParams.y);

        //        float depth = 0.0;
        //        for (int i = -_BlurRadius; i <= _BlurRadius; i++) {
        //            depth += tex2D(_CameraDepthTexture, uv + float2(0, i) * texelSize).r;
        //        }
        //        depth /= (_BlurRadius * 2 + 1);

        //        return depth;
        //    }
        //    ENDHLSL
        //}
    }
}
