// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

///A night vision device (NVD) is an optoelectronic device that allows 
///images to be produced in levels of light approaching total darkness. 
///The image may be a conversion to visible light of both visible light and near-infrared, 
///while by convention detection of thermal infrared is denoted thermal imaging. 
///The image produced is typically monochrome, e.g. shades of green. 
///NVDs are most often used by the military and law enforcement agencies, 
///Many NVDs also include optical components such as a sacrificial lens,[1] or telescopic lenses or mirrors. 
///An NVD may have an IR illuminator, making it an active as opposed to passive night vision device.
///

Shader "PRISM/DeferredNightVisionShader" {

	Properties {
		_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
		_NVColor ("NV Color", Color) = (0,1,0.1724138,0)
		_TargetWhiteColor ("Target White Color", Color) = (0.0,1.0,0.0,1.0)
		_LightSensitivityMultiplier ("SensitivityMultiplier", Range(0,128)) = 90
		_BaseLightingContribution ("Base Lighting Contribution", Range(0,128)) = 1
	}
	
	SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "IgnoreProjector" = "True"
            "UniversalMaterialType" = "Unlit"
            "RenderPipeline" = "UniversalPipeline"
        }
        LOD 100

        // -------------------------------------
        // Render State Commands
		Cull Off ZWrite Off ZTest Always
        Pass
        {
            Name "PRISM Deferred Night Vision"

            HLSLPROGRAM
            #pragma target 3.0
			#pragma multi_compile _ USE_VIGNETTE

            // -------------------------------------
            // Shader Stages
            #pragma vertex GBufferVisPassVertex
            #pragma fragment GBufferVisPassFragment
            //#pragma fragment GBufferVisPassFragment

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/GlobalSamplers.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            // Declare the GBuffer to sample as an input
            TEXTURE2D_X(_GBuffer0);
            //TEXTURE2D_X(_BlitTexture);
			float4 _BlitTexture_ST;
			float4 _NVColor;
			float4 _TargetWhiteColor;
			float _BaseLightingContribution;
			float _LightSensitivityMultiplier;
            FRAMEBUFFER_INPUT_X_HALF(0);

			float LuminancePRISM(float3 color)
{
				float fmin = min(min(color.r, color.g), color.b);
				float fmax = max(max(color.r, color.g), color.b);
				return (fmax + fmin) / 2.0;
			}

            Varyings GBufferVisPassVertex(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                float4 pos = GetFullScreenTriangleVertexPosition(input.vertexID);
                float2 uv  = GetFullScreenTriangleTexCoord(input.vertexID);

                output.positionCS = pos;
                output.texcoord   = uv;

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 colorFromPreviousRenderPass = LOAD_FRAMEBUFFER_X_INPUT(0, input.positionCS.xy);
                return colorFromPreviousRenderPass;
            }

            float4 GBufferVisPassFragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = input.texcoord;
                #ifndef UNITY_UV_STARTS_AT_TOP
                    uv.y = 1.0 - uv.y;
                #endif

				//fixed4 is cheapest, half4 second, float4 expensive (but really doesn't matter here)
				//float4 col = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_PointClamp, uv, 0);		
                float4 col = LOAD_FRAMEBUFFER_X_INPUT(0, input.positionCS.xy);
				float4 dfse = SAMPLE_TEXTURE2D_X_LOD(_GBuffer0, sampler_PointClamp, uv, 0);			
				
				//Get the luminance of the pixel				
				float lumc = LuminancePRISM (col.rgb);
				
				//Desat + green the image
				col = dot(col, _NVColor);	
				
				//Make bright areas/lights too bright
				col.rgb = lerp(col.rgb, _TargetWhiteColor, lumc * _LightSensitivityMultiplier);
				
				//Add some of the regular diffuse texture based off how bright each pixel is
				col.rgb = lerp(col.rgb, dfse.rgb, lumc+_BaseLightingContribution);
				
				#if USE_VIGNETTE
				//Add vignette
				float dist = distance(uv, float2(0.5,0.5));
				col *= smoothstep(0.5,0.45,dist);
				#endif				
				
				//Increase the brightness of all normal areas by a certain amount
				col.rb = max (col.r - 0.75, 0)*4;
				
				return col;
            }
            ENDHLSL
        }
    }

}
