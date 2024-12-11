Shader "Custom/Caustics"
{
	Properties
    {
		_BaseMap ("Caustics Texture", 2D) = "black" {}
        _CausticsSpeed("Caustics Speed", Vector) = (1, 1, 0, 0)
        _CausticsScale("Caustics Scale", Float) = 1.0
        _CausticsStrength("Caustics Strength", Float) = 1.0
        _CausticsUVRatio("Caustics UV1/UV2 Ratio", Float) = 0.75
        _CausticsSplit("Caustics Chromatic Aberration", Float) = 0.1
        _CausticsLuminanceMaskStrength("Caustics Luminance Mask Strength", Range(0,1)) = 1.0
        _CausticsFadeRadius("Caustics Fade Radius", Float) = 4
        _CausticsFadeStrength("Caustics Fade Strength", Range(0,1)) = 0.5
        _CausticsAngleScale("Caustics Angle Scale", Range(0,1)) = 0.5
	}
	SubShader
    {
		Tags
        {
			"RenderPipeline"="UniversalPipeline"
			"RenderType"="Opaque"
			"Queue"="Geometry"
		}

		HLSLINCLUDE
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

		CBUFFER_START(UnityPerMaterial)
		float4 _BaseMap_ST;
        float3 _LightPosition;
		CBUFFER_END
		ENDHLSL

		Pass
        {
			Name "Unlit"
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Front
            ZTest Always

			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			struct Attributes
            {
				float4 positionOS	: POSITION;
				float2 uv		    : TEXCOORD0;
				float4 color		: COLOR;
			};

			struct Varyings
            {
				float4 positionCS 	: SV_POSITION;
				float2 uv		    : TEXCOORD0;
				float4 color		: COLOR;
			};

			TEXTURE2D(_BaseMap);
			SAMPLER(sampler_BaseMap);
            
            float4 _CausticsSpeed;
            float _CausticsScale;
            float _CausticsStrength;
            float _CausticsUVRatio;
            float _CausticsSplit;
            float _CausticsLuminanceMaskStrength;
            float _CausticsFadeRadius;
            float _CausticsFadeStrength;
            float _CausticsAngleScale;

            half2 Panning(half2 uv, half2 speed, half tiling)
            {
                return (speed * _Time.y) + (uv * tiling);
            }

            half3 SampleCaustics(half2 uv, half split)
            {
                half2 uv1 = uv + half2(split, split);
                half2 uv2 = uv + half2(split, -split);
                half2 uv3 = uv + half2(-split, -split);

                half r = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv1).r;
                half g = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv2).r;
                half b = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv3).r;

                return half3(r, g, b);
            }

            // https://alienryderflex.com/hsp.html
            float SampleLuminance(float3 tex)
            {
                return saturate(sqrt(0.299 * pow(tex.x, 2) + 0.587 * pow(tex.y, 2) + 0.114 * pow(tex.z, 2)));
            }

			Varyings vert(Attributes v) 
            {
				Varyings o;

				VertexPositionInputs positionInputs = GetVertexPositionInputs(v.positionOS.xyz);
				o.positionCS = positionInputs.positionCS;
				o.uv = TRANSFORM_TEX(v.uv, _BaseMap);
				o.color = v.color;
				return o;
			}

			half4 frag(Varyings i) : SV_Target 
            {
                float2 positionNDC = i.positionCS.xy / _ScaledScreenParams.xy;

                #if UNITY_REVERSED_Z
                    real depth = SampleSceneDepth(positionNDC);
                #else
                    real depth = lerp(UNITY_NEAR_CLIP_VALUE, 1, SampleSceneDepth(uv));
                #endif
                
                float3 positionWS = ComputeWorldSpacePosition(positionNDC, depth, UNITY_MATRIX_I_VP);
                float3 positionOS = TransformWorldToObject(positionWS);

                // Use world-space coordinates as the UVs
                // multiplied by direction of the main light source
                // and create multiple panning UVs
                // First get the vertical angle
                float3 L = normalize(positionWS - _LightPosition);
                float verticalAngle = atan2(L.y, length(L.xz));
                float angleScale = 1.0 + verticalAngle * _CausticsAngleScale;
                half2 uv = normalize((positionWS - _LightPosition)).xz * angleScale;
                half2 uv1 = Panning(uv, _CausticsUVRatio * _CausticsSpeed.xy, 1 / _CausticsScale);
                half2 uv2 = Panning(uv, _CausticsSpeed.xy, -1 / _CausticsScale);
                half3 tex1 = SampleCaustics(uv1, _CausticsSplit);
                half3 tex2 = SampleCaustics(uv2, _CausticsSplit);
                half3 caustics = min(tex1, tex2) * _CausticsStrength;

                // Sample the luminance to create a luminance mask
                float3 sceneColor = SampleSceneColor(positionNDC);
                float sceneLuminance = SampleLuminance(sceneColor);
                half luminanceMask = 1;
                if (sceneLuminance * 10 < _CausticsLuminanceMaskStrength) luminanceMask = saturate(sceneLuminance * 10 / _CausticsLuminanceMaskStrength);
                
                // Get the edge mask and bounding box, only draw on surfaces
                float boundingBoxMask = 1;
                boundingBoxMask = all(step(positionOS, 0.5) * (1 - step(positionOS, -0.5)));
                half edgeFadeMask = 1 - saturate((distance(positionOS, 0) - _CausticsFadeRadius) / (1 - _CausticsFadeStrength));

                return float4(caustics * luminanceMask, SampleLuminance(saturate(caustics * luminanceMask)) * boundingBoxMask * edgeFadeMask);
			}
			ENDHLSL
		}
	}
}