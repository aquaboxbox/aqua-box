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
            #pragma vertex BillboardVert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"
            #include "BillboardVert.hlsl"

            float _DepthRounding;

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
            #pragma vertex BillboardVert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"
            #include "BillboardVert.hlsl"

            float _Thickness;

            float4 frag(VertexOutput vIn): SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(vIn);

                float2 normUv = vIn.uv * 2.0 - 1.0;
                float dist = length(normUv);

                bool inMask = dist >= 1.0;
                if (inMask) {
                    discard;
                }

                float thickness = (1.0 - dist * dist) * _Thickness;

                return float4(1,1,1,thickness);

            }
            ENDCG

        }

        // bilateral H
        Pass {
            ZWrite On
            ZTest Off

            HLSLPROGRAM

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Gaussian.hlsl"

            #pragma vertex Vert     // take vert from blit package
            #pragma fragment frag

            sampler2D _DepthTexture;
            int _BlurRadius;
            float _IntensitySigma;
            float _DistanceSigma;

            float frag(Varyings input) : SV_Depth
            {
                float2 uv = input.texcoord.xy;
                float2 texelSize = 1.0 / float2(_ScreenParams.x, _ScreenParams.y);
                float depth = tex2D(_DepthTexture, uv).r;
                int kernelSize = _BlurRadius;
                // TODO: dynamically set kernelsize with depth

                float sum = 0.0;
                float weightSum = 0.0;
                for (int i = -kernelSize; i <= kernelSize; i++) {
                        float2 sampleUv = uv + float2(i,0) * texelSize;
                        float sampleDepth = tex2D(_DepthTexture, sampleUv).r;

                        float spatial = gaussian1(length(float2(i,0)), _DistanceSigma);
                        float intensity = gaussian1(abs(depth - sampleDepth), _IntensitySigma);

                        float weight = spatial * intensity;

                        sum += sampleDepth * weight;
                        weightSum += weight;
                }
                sum /= weightSum;

                return sum;
            }
            ENDHLSL
        }

        // bilateral V
        Pass {
            ZWrite On
            ZTest Off

            HLSLPROGRAM

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Gaussian.hlsl"

            #pragma vertex Vert     // take vert from blit package
            #pragma fragment frag

            sampler2D _DepthHorizontalTexture;
            int _BlurRadius;
            float _IntensitySigma;
            float _DistanceSigma;

            float frag(Varyings input) : SV_Depth
            {
                float2 uv = input.texcoord.xy;
                float2 texelSize = 1.0 / float2(_ScreenParams.x, _ScreenParams.y);
                float depth = tex2D(_DepthHorizontalTexture, uv).r;
                int kernelSize = _BlurRadius;
                // TODO: dynamically set kernelsize with depth

                float sum = 0.0;
                float weightSum = 0.0;
                for (int i = -kernelSize; i <= kernelSize; i++) {
                        float2 sampleUv = uv + float2(0,i) * texelSize;
                        float sampleDepth = tex2D(_DepthHorizontalTexture, sampleUv).r;

                        float spatial = gaussian1(length(float2(0,i)), _DistanceSigma);
                        float intensity = gaussian1(abs(depth - sampleDepth), _IntensitySigma);

                        float weight = spatial * intensity;

                        sum += sampleDepth * weight;
                        weightSum += weight;
                }
                sum /= weightSum;

                return sum;
            }
            ENDHLSL
        }

        // gaussian H
        Pass {
            ZWrite Off
            ZTest Off

            HLSLPROGRAM

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Gaussian.hlsl"

            #pragma vertex Vert     // take vert from blit package
            #pragma fragment frag

            sampler2D _ThicknessTexture;
            int _ThicknessBlurRadius;
            float _ThicknessSigma;

            float4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord.xy;
                float2 texelSize = 1.0 / float2(_ScreenParams.x, _ScreenParams.y);
                float4 color = tex2D(_ThicknessTexture, uv);
                int kernelSize = _ThicknessBlurRadius;

                float4 sum = float4(0,0,0,0);
                float weightSum = 0.0;
                for (int i = -kernelSize; i <= kernelSize; i++) {
                    float2 sampleUv = uv + float2(i,0) * texelSize;

                    float4 sampleColor = tex2D(_ThicknessTexture, sampleUv);
                    float weight = gaussian1(length(float2(i,0)), _ThicknessSigma);

                    sum += sampleColor * weight;
                    weightSum += weight;
                }
                sum /= weightSum;

                return sum;
            }
            ENDHLSL
        }
        // gaussian V
        Pass {
            ZWrite Off
            ZTest Off

            HLSLPROGRAM

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Gaussian.hlsl"

            #pragma vertex Vert     // take vert from blit package
            #pragma fragment frag

            sampler2D _ThicknessHorizontalTexture;
            int _ThicknessBlurRadius;
            float _ThicknessSigma;

            float4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord.xy;
                float2 texelSize = 1.0 / float2(_ScreenParams.x, _ScreenParams.y);
                float4 color = tex2D(_ThicknessHorizontalTexture, uv);
                int kernelSize = _ThicknessBlurRadius;

                float4 sum = float4(0,0,0,0);
                float weightSum = 0.0;
                for (int i = -kernelSize; i <= kernelSize; i++) {
                    float2 sampleUv = uv + float2(0,i) * texelSize;

                    float4 sampleColor = tex2D(_ThicknessHorizontalTexture, sampleUv);
                    float weight = gaussian1(length(float2(0,i)), _ThicknessSigma);

                    sum += sampleColor * weight;
                    weightSum += weight;
                }
                sum /= weightSum;

                return sum;
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
            sampler2D _DepthVerticalTexture;
            sampler2D _ThicknessVerticalTexture;
            sampler2D _ColorTexture;

            float4x4 _InverseView;
            float4x4 _InverseProjection;
            float4x4 _InverseViewProjection;
            float3 _LightPos;
            float _SpecularHighlight;
            float _RefractionCoefficient;
            float4 _FluidColor;
            float _Absorption;

            #pragma vertex Vert     // take vert from blit package
            #pragma fragment frag

            float fresnel(float cosTheta, float p)
            {
                return pow(saturate(1.0 - cosTheta), p);
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
                float2 uv = input.texcoord;

                // generate normal from depth
                // https://wickedengine.net/2019/09/improved-normal-reconstruction-from-depth/

                float2 texelSize = 1.0 / float2(_ScreenParams.x, _ScreenParams.y);
                float centerDepth = tex2D(_DepthVerticalTexture, uv + float2(0, 0) * texelSize).r;
                float leftDepth   = tex2D(_DepthVerticalTexture, uv + float2(-1,0) * texelSize).r;
                float rightDepth  = tex2D(_DepthVerticalTexture, uv + float2(1, 0) * texelSize).r;
                float topDepth    = tex2D(_DepthVerticalTexture, uv + float2(0,-1) * texelSize).r;
                float bottomDepth = tex2D(_DepthVerticalTexture, uv + float2(0, 1) * texelSize).r;

                float3 centerPos = reconstructPosition(uv + float2(0, 0) * texelSize, centerDepth);
                float3 leftPos   = reconstructPosition(uv + float2(-1,0) * texelSize, leftDepth);
                float3 rightPos  = reconstructPosition(uv + float2(1, 0) * texelSize, rightDepth);
                float3 topPos    = reconstructPosition(uv + float2(0,-1) * texelSize, topDepth);
                float3 bottomPos = reconstructPosition(uv + float2(0, 1) * texelSize, bottomDepth);

                bool leftBest = abs(leftDepth - centerDepth) < abs(rightDepth - centerDepth);
                bool topBest = abs(topDepth - centerDepth) < abs(bottomDepth - centerDepth);

                float3 P0 = centerPos;
                float3 P1;
                float3 P2;
                if (leftBest  && topBest)  { P1 = leftPos; P2 = topPos; }       // top left
                if (!leftBest && topBest)  { P1 = topPos; P2 = rightPos; }      // top right
                if (leftBest  && !topBest) { P1 = bottomPos; P2 = leftPos; }    // bottom left
                if (!leftBest && !topBest) { P1 = rightPos; P2 = bottomPos; }   // bottom right

                float3 normal = normalize(cross(P2 - P0, P1 - P0));

                // simple specular reflections
                
                float3 lightPos = _LightPos;
                float3 lightDir = normalize(lightPos - centerPos);
                float3 viewDir = normalize(_WorldSpaceCameraPos - centerPos);
                float3 halfDir = normalize(lightDir + viewDir);
                float specular = 0.5 * pow(saturate(dot(halfDir, normal)), _SpecularHighlight);

                //float ambient  = 0.1;
                //float diffuse  = 0.8 * saturate(dot(lightDir, normal));

                // beers law + refraction
                float thickness = tex2D(_ThicknessVerticalTexture, uv).r;
                float4 baseSceneColor = tex2D(_ColorTexture, uv);
                float2 refractedUv = uv + normal.xy * thickness * _RefractionCoefficient;
                float4 refractedSceneColor = tex2D(_ColorTexture, refractedUv);
                float refractedDepth = tex2D(_CameraDepthTexture, refractedUv).r;
                // Don't refract object in front of water
                if (refractedDepth > centerDepth) {
                    refractedSceneColor = baseSceneColor;
                }

                // TODO: modify rgb channels with thickness differently instead of hardcoding color
                float3 fluidColor = _FluidColor.rgb;
                float3 refractedColor = lerp(fluidColor, refractedSceneColor.xyz, exp(-thickness * _Absorption));
                float3 color = refractedColor + specular;

                //float fresnelFactor = fresnel(dot(normal, viewDir), 15.0);
                //float3 reflectedColor = float3(1,1,1);
                //float fresnelFactor = fresnel(dot(normal, viewDir), 5.0);
                //float3 color = refractedColor * (1.0 - fresnel(dot(normal, viewDir))) + reflectedColor * fresnel(dot(normal, viewDir)) + specular;
                //float3 color = diffuse * float3(1,1,1);

                // manual depth test
                // TODO: can probably move this before lighting calculations to save computations
                float fluidDepth = centerDepth;
                float sceneDepth = tex2D(_CameraDepthTexture, uv).r;
                if (sceneDepth > fluidDepth) {
                    return baseSceneColor;
                }

                return float4(color, 1);
                //return float4(fresnelFactor, fresnelFactor, fresnelFactor, 1);
                //return float4(normal, 1);
                //return float4(centerPos, 1);
                //return float4(centerDepth, centerDepth, centerDepth, 1);
            }
            ENDHLSL
        }
    }
}
