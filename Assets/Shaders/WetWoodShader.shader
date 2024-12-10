Shader "Custom/WetWoodShader"
{
	Properties
    {
		[MainTexture] _BaseMap("Base Map (RGB) Smoothness / Alpha (A)", 2D) = "white" {}
		[MainColor]   _BaseColor("Base Color", Color) = (1, 1, 1, 1)

		[Toggle(_NORMALMAP)] _NormalMapToggle ("Normal Mapping", Float) = 0.0
		[NoScaleOffset] _BumpMap("Normal Map", 2D) = "bump" {}

		[HDR] _EmissionColor("Emission Color", Color) = (0,0,0)
		[Toggle(_EMISSION)] _Emission ("Emission", Float) = 0.0
		[NoScaleOffset]_EmissionMap("Emission Map", 2D) = "white" {}

		[Toggle(_ALPHATEST_ON)] _AlphaTestToggle ("Alpha Clipping", Float) = 0.0
		_Cutoff ("Alpha Cutoff", Float) = 0.5

		[Toggle(_SPECGLOSSMAP)] _SpecGlossMapToggle ("Use Specular Gloss Map", Float) = 0.0
		_SpecColor("Specular Color", Color) = (0.5, 0.5, 0.5, 0.5)
		_SpecGlossMap("Specular Map", 2D) = "white" {}
		[Toggle(_GLOSSINESS_FROM_BASE_ALPHA)] _GlossSource ("Glossiness Source, from Albedo Alpha (if on) vs from Specular (if off)", Float) = 1
		_Smoothness("Smoothness", Range(0.0, 1.0)) = 0.0
        
		[Header(Wet Floor Properties)]
        _WetnessMap("Wetness Map", 2D) = "black" {}
		_DarkeningFactor("Floor Darkening Factor", Range(0,1)) = 0.7
        _GlossinessCutoff("Glossiness Cutoff", Range(0,1)) = 0.7
        _FresnelEdgeScale("Fresnel Edge Mask Scale", Float) = 5.0
        _FresnelPower("Fresnel Power", Float) = 5.0
        _FresnelStrength("Fresnel Strength", Range(0,1)) = 0.5
        [Toggle] _FresnelActive("Fresnel Active", Float) = 0.0
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

		CBUFFER_START(UnityPerMaterial)
		float4 _BaseMap_ST;
		float4 _BaseColor;
		float4 _EmissionColor;
		float4 _SpecColor;
		float _Cutoff;
		float _Smoothness;
		CBUFFER_END
		ENDHLSL

		Pass 
        {
			Name "ForwardLit"
			Tags { "LightMode"="UniversalForward" }
			Blend SrcAlpha OneMinusSrcAlpha

			HLSLPROGRAM
			#pragma vertex LitPassVertex
			#pragma fragment LitPassFragment

			// Material Keywords
			#pragma shader_feature_local _NORMALMAP
			#pragma shader_feature_local_fragment _EMISSION
			#pragma shader_feature_local _RECEIVE_SHADOWS_OFF
			//#pragma shader_feature_local_fragment _SURFACE_TYPE_TRANSPARENT
			#pragma shader_feature_local_fragment _ALPHATEST_ON
			#pragma shader_feature_local_fragment _ALPHAPREMULTIPLY_ON
			//#pragma shader_feature_local_fragment _ _SPECGLOSSMAP _SPECULAR_COLOR
			#pragma shader_feature_local_fragment _ _SPECGLOSSMAP
			#define _SPECULAR_COLOR // always on
			#pragma shader_feature_local_fragment _GLOSSINESS_FROM_BASE_ALPHA

			// URP Keywords
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
			// Note, v11 changes this to :
			// #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN

			#pragma multi_compile _ _SHADOWS_SOFT
			#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
			#pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
			#pragma multi_compile _ LIGHTMAP_SHADOW_MIXING // v10+ only, renamed from "_MIXED_LIGHTING_SUBTRACTIVE"
			#pragma multi_compile _ SHADOWS_SHADOWMASK // v10+ only

			// Unity Keywords
			#pragma multi_compile _ LIGHTMAP_ON
			#pragma multi_compile _ DIRLIGHTMAP_COMBINED
			#pragma multi_compile_fog

			// GPU Instancing (not supported)
			//#pragma multi_compile_instancing

			// Includes
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

			// Structs
			struct Attributes
            {
				float4 positionOS	: POSITION;
				float4 normalOS		: NORMAL;
				#ifdef _NORMALMAP
					float4 tangentOS 	: TANGENT;
				#endif
				float2 uv		    : TEXCOORD0;
				float2 lightmapUV	: TEXCOORD1;
				float4 color		: COLOR;
				//UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct Varyings
            {
				float4 positionCS 					: SV_POSITION;
				float2 uv		    				: TEXCOORD0;
				DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 1);
				float3 positionWS					: TEXCOORD2;

				#ifdef _NORMALMAP
					half4 normalWS					: TEXCOORD3;    // xyz: normal, w: viewDir.x
					half4 tangentWS					: TEXCOORD4;    // xyz: tangent, w: viewDir.y
					half4 bitangentWS				: TEXCOORD5;    // xyz: bitangent, w: viewDir.z
				#else
					half3 normalWS					: TEXCOORD3;
				#endif
				
				#ifdef _ADDITIONAL_LIGHTS_VERTEX
					half4 fogFactorAndVertexLight	: TEXCOORD6; // x: fogFactor, yzw: vertex light
				#else
					half  fogFactor					: TEXCOORD6;
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
					float4 shadowCoord 				: TEXCOORD7;
				#endif

                float4 screenPos 					: TEXCOORD8;
				float4 color						: COLOR;
			};

			// Textures, Samplers & Global Properties
			// (note, BaseMap, BumpMap and EmissionMap is being defined by the SurfaceInput.hlsl include)
			TEXTURE2D(_SpecGlossMap); SAMPLER(sampler_SpecGlossMap);
            TEXTURE2D(_CameraOpaqueTexture); SAMPLER(sampler_CameraOpaqueTexture);
            TEXTURE2D(_WetnessMap); SAMPLER(sampler_WetnessMap);
			float _DarkeningFactor;
			float _GlossinessCutoff;
			float _FresnelEdgeScale;
            float _FresnelPower;
            float _FresnelStrength;
			float _FresnelActive;

			// Functions
			half4 SampleSpecularSmoothness(float2 uv, half alpha, half4 specColor, TEXTURE2D_PARAM(specMap, sampler_specMap))
            {
				half4 specularSmoothness = half4(0.0h, 0.0h, 0.0h, 1.0h);
				#ifdef _SPECGLOSSMAP
					specularSmoothness = SAMPLE_TEXTURE2D(specMap, sampler_specMap, uv) * specColor;
				#elif defined(_SPECULAR_COLOR)
					specularSmoothness = specColor;
				#endif

				#if UNITY_VERSION >= 202120 // or #if SHADER_LIBRARY_VERSION_MAJOR < 12, but that versioning method is deprecated for newer versions
					// v12 is changing this, so it's calculated later. Likely so that smoothness value stays 0-1 so it can display better for debug views.
					#ifdef _GLOSSINESS_FROM_BASE_ALPHA
						specularSmoothness.a = exp2(10 * alpha + 1);
					#else
						specularSmoothness.a = exp2(10 * specularSmoothness.a + 1);
					#endif
				#endif
				return specularSmoothness;
			}

			//  SurfaceData & InputData
			void InitalizeSurfaceData(Varyings IN, out SurfaceData surfaceData)
            {
				surfaceData = (SurfaceData)0; // avoids "not completely initalized" errors

				half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);

				#ifdef _ALPHATEST_ON
					// Alpha Clipping
					clip(baseMap.a - _Cutoff);
				#endif

				half4 diffuse = baseMap * _BaseColor * IN.color;
				surfaceData.albedo = diffuse.rgb;
				surfaceData.normalTS = SampleNormal(IN.uv, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap));
				surfaceData.emission = SampleEmission(IN.uv, _EmissionColor.rgb, TEXTURE2D_ARGS(_EmissionMap, sampler_EmissionMap));
				surfaceData.occlusion = 1.0; // unused

				half4 specular = SampleSpecularSmoothness(IN.uv, diffuse.a, _SpecColor, TEXTURE2D_ARGS(_SpecGlossMap, sampler_SpecGlossMap));
				surfaceData.specular = specular.rgb;
				surfaceData.smoothness = specular.a * _Smoothness;
			}

			void InitializeInputData(Varyings input, half3 normalTS, out InputData inputData)
            {
				inputData = (InputData)0; // avoids "not completely initalized" errors

				inputData.positionWS = input.positionWS;

				#ifdef _NORMALMAP
					half3 viewDirWS = half3(input.normalWS.w, input.tangentWS.w, input.bitangentWS.w);
					inputData.normalWS = TransformTangentToWorld(normalTS,half3x3(input.tangentWS.xyz, input.bitangentWS.xyz, input.normalWS.xyz));
				#else
					half3 viewDirWS = GetWorldSpaceNormalizeViewDir(inputData.positionWS);
					inputData.normalWS = input.normalWS;
				#endif

				inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);

				viewDirWS = SafeNormalize(viewDirWS);
				inputData.viewDirectionWS = viewDirWS;

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
					inputData.shadowCoord = input.shadowCoord;
				#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
					inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
				#else
					inputData.shadowCoord = float4(0, 0, 0, 0);
				#endif

				// Fog
				#ifdef _ADDITIONAL_LIGHTS_VERTEX
					inputData.fogCoord = input.fogFactorAndVertexLight.x;
					inputData.vertexLighting = input.fogFactorAndVertexLight.yzw;
				#else
					inputData.fogCoord = input.fogFactor;
					inputData.vertexLighting = half3(0, 0, 0);
				#endif

				/* in v11/v12?, could use :
				#ifdef _ADDITIONAL_LIGHTS_VERTEX
					inputData.fogCoord = InitializeInputDataFog(float4(inputData.positionWS, 1.0), input.fogFactorAndVertexLight.x);
					inputData.vertexLighting = input.fogFactorAndVertexLight.yzw;
				#else
					inputData.fogCoord = InitializeInputDataFog(float4(inputData.positionWS, 1.0), input.fogFactor);
					inputData.vertexLighting = half3(0, 0, 0);
				#endif
				*/

				inputData.bakedGI = SAMPLE_GI(input.lightmapUV, input.vertexSH, inputData.normalWS);
				inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
				inputData.shadowMask = SAMPLE_SHADOWMASK(input.lightmapUV);
			}

			// Vertex Shader
			Varyings LitPassVertex(Attributes IN)
            {
				Varyings OUT;

				//UNITY_SETUP_INSTANCE_ID(IN);
				//UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
				//UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

				VertexPositionInputs positionInputs = GetVertexPositionInputs(IN.positionOS.xyz);
				#ifdef _NORMALMAP
					VertexNormalInputs normalInputs = GetVertexNormalInputs(IN.normalOS.xyz, IN.tangentOS);
				#else
					VertexNormalInputs normalInputs = GetVertexNormalInputs(IN.normalOS.xyz);
				#endif
				

				OUT.positionCS = positionInputs.positionCS;
				OUT.positionWS = positionInputs.positionWS;

				half3 viewDirWS = GetWorldSpaceViewDir(positionInputs.positionWS);
				half3 vertexLight = VertexLighting(positionInputs.positionWS, normalInputs.normalWS);
				half fogFactor = ComputeFogFactor(positionInputs.positionCS.z);
				
				#ifdef _NORMALMAP
					OUT.normalWS = half4(normalInputs.normalWS, viewDirWS.x);
					OUT.tangentWS = half4(normalInputs.tangentWS, viewDirWS.y);
					OUT.bitangentWS = half4(normalInputs.bitangentWS, viewDirWS.z);
				#else
					OUT.normalWS = NormalizeNormalPerVertex(normalInputs.normalWS);
					//OUT.viewDirWS = viewDirWS;
				#endif

				OUTPUT_LIGHTMAP_UV(IN.lightmapUV, unity_LightmapST, OUT.lightmapUV);
				OUTPUT_SH(OUT.normalWS.xyz, OUT.vertexSH);

				#ifdef _ADDITIONAL_LIGHTS_VERTEX
					OUT.fogFactorAndVertexLight = half4(fogFactor, vertexLight);
				#else
					OUT.fogFactor = fogFactor;
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
					OUT.shadowCoord = GetShadowCoord(positionInputs);
				#endif

				OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.screenPos = ComputeScreenPos(OUT.positionCS);
				OUT.color = IN.color;
				return OUT;
			}

			// Fragment Shader
			half4 LitPassFragment(Varyings IN) : SV_Target
            {
                float currentWetness = SAMPLE_TEXTURE2D(_WetnessMap, sampler_WetnessMap, IN.uv).r;
                float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
                float4 sceneColor = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, screenUV);
				float albedoAlpha = 0;

				if (currentWetness > _GlossinessCutoff) {
					albedoAlpha = saturate(currentWetness - 0.5);
				}
				else if (currentWetness > 0) {
					albedoAlpha = currentWetness * (currentWetness - 0.5);
					_Smoothness = saturate(_Smoothness + (_GlossinessCutoff - currentWetness) * (1 - _Smoothness));
				}

				// Setup SurfaceData
				SurfaceData surfaceData;
				InitalizeSurfaceData(IN, surfaceData);
				surfaceData.albedo = float4(sceneColor.xyz * (1.0 - currentWetness * _DarkeningFactor), albedoAlpha);
                surfaceData.alpha = currentWetness;
				
                float fresnel = 0;
				if (_FresnelActive > 0 && currentWetness > 0) {
					// Treat the wetness as a height map to derive normals
					float2 offset = float2(0.001, 0.001);
					float wet_x = SAMPLE_TEXTURE2D(_WetnessMap, sampler_WetnessMap, IN.uv + float2(offset.x, 0)).r;
					float wet_y = SAMPLE_TEXTURE2D(_WetnessMap, sampler_WetnessMap, IN.uv + float2(0, offset.y)).r;
					float dfdx = wet_x - currentWetness;
					float dfdy = wet_y - currentWetness;
					float3 wetNormalTS = normalize(float3(-dfdx, -dfdy, 1.0));
					surfaceData.normalTS = normalize(lerp(surfaceData.normalTS, wetNormalTS, currentWetness));

					// Fresnel for edge reflections
					fresnel = pow(1.0 - saturate(dot(normalize(surfaceData.normalTS), normalize(GetCameraPositionWS() - IN.positionWS))), _FresnelPower);

					// Use the wetness mask as an outline for the fresnel
					dfdx = ddx(currentWetness);
					dfdy = ddy(currentWetness);
					float edgeMask = abs(dfdx) + abs(dfdy);
					edgeMask = saturate(edgeMask * _FresnelEdgeScale);

					fresnel *= _FresnelStrength * currentWetness * edgeMask;
				}

                surfaceData.emission = fresnel;

				// Setup InputData
				InputData inputData;
				InitializeInputData(IN, surfaceData.normalTS, inputData);

				// Simple Lighting (Lambert & BlinnPhong)
				half4 color = UniversalFragmentBlinnPhong(inputData, surfaceData);

				color.rgb = MixFog(color.rgb, inputData.fogCoord);
                return half4(color.rgb, currentWetness);
			}
			ENDHLSL
		}
	}
}