Shader "Custom/BarrelFillShader"
{
    Properties
    {
        _ColorTop ("Top Color", Color) = (0.5, 0.5, 0.5, 1) // Grey
        _ColorBottom ("Bottom Color", Color) = (0.0, 0.0, 1.0, 1) // Blue
        _MinY ("Minimum Y", Float) = -5.0 // Minimum Y value
        _MaxY ("Maximum Y", Float) = 50.0 // Maximum Y value
        _FillLevel ("Fill Level", Range(0,1)) = 0.5 // Fill level between min and max
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }
        LOD 100

        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION; // Object space position
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION; // Homogeneous clip space position
                float worldY : TEXCOORD0;        // Y-coordinate in world space
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _ColorTop;     // Top color of the barrel
                float4 _ColorBottom;  // Bottom color of the barrel
                float _MinY;          // Minimum Y value
                float _MaxY;          // Maximum Y value
                float _FillLevel;     // Fill level between min and max
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;

                // Convert object space position to homogeneous clip space
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz); // Convert to world space
                output.positionHCS = TransformWorldToHClip(positionWS);           // Convert to clip space

                // Pass the world Y position to the fragment shader
                output.worldY = positionWS.y;

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float minY = min(_MinY, _MaxY);
                float maxY = max(_MinY, _MaxY);

                // Calculate the fill Y position based on the fill level
                float fillY = lerp(minY, maxY, _FillLevel);

                // Determine if the current Y position is below the fill level
                float inFill = step(input.worldY, fillY);

                // Blend between the two colors based on inFill
                return lerp(_ColorTop, _ColorBottom, inFill);
            }
            ENDHLSL
        }
    }
    FallBack "Diffuse"
}
