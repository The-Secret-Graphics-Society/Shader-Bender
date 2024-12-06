Shader "Custom/WaterShader"
{
    Properties
    {
		[MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [Header(Surface Displacement)]
        _VertexNoiseScale("Vertex Noise Scale", Float) = 5.0
        _VertexDisplacement("Vertex Displacement", Float) = 1.0
        _DistortionScale("Surface Distortion Scale", Float) = 10.0
        _Distortion("Distortion", Float) = 1.0
        
        [Header(Normal Maps)]
		_BumpMap("Normal Map", 2D) = "bump" {}
		_BumpScale("Normal Strength", Float) = 1.0
        _NormalTiling("Normal Tiling", Float) = 1.0
        _NormalSpeed("Normal Speed", Float) = 1.0
        
        [Header(Other Properties)]
        _FresnelPower("Fresnel Power", Float) = 5.0
        _Smoothness("Smoothness", Range(0,1)) = 0.5
        _Dissolve("Dissolve", Range(0,1)) = 0.0
        _DissolveSpeed("Dissolve Speed", Float) = 0.2
        _DissolveScale("Dissolve Scale", Float) = 0.4
		_SpecColor("Specular Color", Color) = (0.5, 0.5, 0.5, 0.5)
		_Cutoff ("Alpha Cutoff", Float) = 0.5
    }
    SubShader
    {
        Tags {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
        }
        LOD 200

        HLSLINCLUDE
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

		CBUFFER_START(UnityPerMaterial)
		float4 _BaseMap_ST;
		float4 _BaseColor;
		float4 _EmissionColor;
		float4 _SpecColor;
		float _Metallic;
		float _Smoothness;
		float _OcclusionStrength;
		float _Cutoff;
		float _BumpScale;
		CBUFFER_END
		ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // Material Keywords
			#pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local_fragment _ALPHATEST_ON
			#pragma shader_feature_local_fragment _ALPHAPREMULTIPLY_ON
			#pragma shader_feature_local_fragment _SPECULAR_SETUP
			#pragma shader_feature_local _RECEIVE_SHADOWS_OFF

			// URP Keywords
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
			#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
			#pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
			#pragma multi_compile_fragment _ _SHADOWS_SOFT
			#pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
			#pragma multi_compile _ SHADOWS_SHADOWMASK

			// Unity Keywords
			#pragma multi_compile _ LIGHTMAP_ON
			#pragma multi_compile _ DIRLIGHTMAP_COMBINED

            // Properties
            float _VertexNoiseScale;
            float _VertexDisplacement;
            float _DistortionScale;
            float _Distortion;
            TEXTURE2D(_CameraOpaqueTexture);
            SAMPLER(sampler_CameraOpaqueTexture);
            float _NormalTiling;
            float _NormalSpeed;
            float _FresnelPower;
            float _Dissolve;
            float _DissolveSpeed;
            float _DissolveScale;
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float3 positionOS : POSITION;
                float4 tangentOS : TANGENT;
                float3 normalOS : NORMAL;
                float4 uv : TEXCOORD0;
				float4 color : COLOR;
                float4 custom1 : TEXCOORD1;
				float2 lightmapUV : TEXCOORD2;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 uv : TEXCOORD0;
				DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 1);
				float3 positionOS : TEXCOORD2;
				float3 positionWS : TEXCOORD3;
                half4 normalWS : TEXCOORD4;
                half4 tangentWS : TEXCOORD5;
                half4 bitangentWS : TEXCOORD6;
				
				#ifdef _ADDITIONAL_LIGHTS_VERTEX
					half4 fogFactorAndVertexLight : TEXCOORD8;
				#else
					half fogFactor : TEXCOORD8;
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
					float4 shadowCoord : TEXCOORD9;
				#endif

				float4 color : COLOR;
                float4 screenPos : TEXCOORD10;
            };

            #include "Includes/3DSimplexNoise.hlsl"
            #include "Includes/GradientNoise.hlsl"
            #include "Includes/PBRInput.hlsl"
            #include "Includes/PBRSurface.hlsl"
            #include "Includes/SimpleNoise.hlsl"
            #include "Includes/TriplanarNormalMapping.hlsl"

            Varyings vert(Attributes v)
            {
                Varyings o = (Varyings)0;

                // Vertex displacement
                float3 pos = TransformObjectToWorld(v.positionOS) * _VertexNoiseScale;
                float noise = SimplexNoise(pos + _Time.y * 0.5);
                noise = noise * 2.0 - 1.0;
                float displacement = noise * _VertexDisplacement;
                float3 displacementVector = displacement * v.normalOS;
                float3 displacedPositionOS = v.positionOS + displacementVector;
                float3 displacedPositionWS = TransformObjectToWorld(displacedPositionOS);

                // Stuff from the PBR Lit Template
                VertexPositionInputs positionInputs = GetVertexPositionInputs(displacedPositionOS.xyz);
				VertexNormalInputs normalInputs = GetVertexNormalInputs(normalize(TransformObjectToWorldNormal(v.normalOS)), v.tangentOS);
                half3 viewDirWS = GetWorldSpaceViewDir(positionInputs.positionWS);
                
                o.positionCS = TransformWorldToHClip(displacedPositionWS);
                o.uv = v.uv;
                o.uv.w = v.custom1.x;
                o.normalWS = half4(normalInputs.normalWS, viewDirWS.x);
                o.tangentWS = half4(normalInputs.tangentWS, viewDirWS.y);
                o.bitangentWS = half4(normalInputs.bitangentWS, viewDirWS.z);
                o.positionOS = displacedPositionOS;
                o.positionWS = displacedPositionWS;
				o.color = v.color;
                o.screenPos = ComputeScreenPos(o.positionCS);

                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                // - Distortion effect - 
                // Sample scene color with distortion
                float3 pos = i.positionWS * _DistortionScale;
                float noise = SimplexNoise(pos + _Time.y * 0.5);
                noise = noise * 2.0 - 1.0;
                float distortion = noise * _Distortion;
                float2 screenUV = i.screenPos.xy / i.screenPos.w;
                float4 sceneColor = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, screenUV + distortion);
                _BaseColor = sceneColor;

                // - Normal mapping -
                // Time-based offsets for normal mapping
                float normalOffset1 = _Time.y * _NormalSpeed * 0.1;
                float normalOffset2 = _Time.y * _NormalSpeed * -0.05;

                // Sample the normal map textures
                float3 normalSample1 = TriplanarNormalMapping(i.positionWS, i.normalWS, _NormalTiling, normalOffset1);
                float3 normalSample2 = TriplanarNormalMapping(i.positionWS, i.normalWS, (_NormalTiling * 0.5), normalOffset2);
                float3 normal = (normalSample1 + normalSample2) * _BumpScale;
                
                // - Fresnel effect -
                float fresnel = pow(1.0 - saturate(dot(normalize(i.normalWS), normalize(GetCameraPositionWS() - i.positionWS))), _FresnelPower);

                // - Dissolving -
                float2 offsetUV = (i.uv.xy + i.uv.w)  + (0, _Time.y * _DissolveSpeed);
                float simpleNoise =  0.1 + SimpleNoise(offsetUV, _DissolveScale) * 0.9;
                float steppedNoise = step((1 - i.color.a), simpleNoise);

                // - PBR stuff -
                SurfaceData surfaceData;
				InitializeSurfaceData(i, surfaceData);
                surfaceData.albedo = sceneColor.rgb;
                surfaceData.specular = _SpecColor.rgb;
                surfaceData.metallic = 0.0;
                surfaceData.smoothness = _Smoothness;
                surfaceData.normalTS = normal;
                surfaceData.emission = sceneColor + fresnel;
                surfaceData.alpha = steppedNoise;

				InputData inputData;
				InitializeInputData(i, surfaceData.normalTS, inputData);
                
                float3 color = UniversalFragmentPBR(inputData, surfaceData);

                return float4(color, surfaceData.alpha);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/InternalErrorShader"
}
