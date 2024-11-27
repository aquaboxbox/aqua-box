Shader "Custom/BarrelFillShader"
{
    Properties
    {
        _ColorTop ("Top Color", Color) = (0.5, 0.5, 0.5, 1)       // Grey
        _ColorBottom ("Bottom Color", Color) = (0.0, 0.0, 1.0, 1)  // Blue
        _FillLevel ("Fill Level", Range(0,1)) = 0.0                // Fill level from 0 to 1
        // Note: _MinY and _MaxY are hidden since they are set via script
        [HideInInspector]_MinY ("Minimum Y", Float) = 0.0          // Minimum Y of the object
        [HideInInspector]_MaxY ("Maximum Y", Float) = 1.0          // Maximum Y of the object
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
                float objectY : TEXCOORD0;        // Y-coordinate in object space
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _ColorTop;        // Color above the fill level
                float4 _ColorBottom;     // Color below the fill level
                float _FillLevel;        // Fill level from 0 to 1
                float _MinY;             // Minimum Y of the object
                float _MaxY;             // Maximum Y of the object
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                // Transform object space position to clip space
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                // Pass the object space Y position to the fragment shader
                output.objectY = input.positionOS.y;

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Normalize the objectY coordinate between 0 and 1
                float normalizedY = (input.objectY - _MinY) / (_MaxY - _MinY);
                normalizedY = saturate(normalizedY);

                // Determine if the current Y position is below the fill level
                float inFill = step(normalizedY, _FillLevel);

                // Blend between the two colors based on inFill
                return lerp(_ColorTop, _ColorBottom, inFill);
            }
            ENDHLSL
        }
    }
    FallBack "Diffuse"
}
