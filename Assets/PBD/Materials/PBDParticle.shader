Shader "PBDParticle" 
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

			// Include logic file
			#include "Particle.hlsl"

			ENDHLSL
		}
	}
}
