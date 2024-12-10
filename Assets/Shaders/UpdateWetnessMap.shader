Shader "Custom/UpdateWetnessMap"
{
    Properties
    {
        _WetnessMap("Wetness Map", 2D) = "black" {}
        _LeftHandPos("Left Hand Position", Vector) = (0, 0, 0)
        _RightHandPos("Right Hand Position", Vector) = (0, 0, 0)
        [Toggle] _WaterActive("Water Active", Float) = 0.0
        _Darkness("Darkness Amount", Range(0, 1)) = 0.05
        _Range("Wetness Range", Float) = 0.5
        _FadeSpeed("Fade Speed", Float) = 0.2
        _NoiseScale("Noise Scale", Float) = 50.0

        _WorldOrigin("World Origin", Vector) = (0,0,0)
        _WorldScale("World Scale", Vector) = (1,1,1)
        _WorldRotationY("World Rotation in Radians around Y", Float) = 0.0
    }

    SubShader
    {
        Lighting Off
        Blend One Zero

        Pass
        {
            Name "UpdateWetness"
            Blend One Zero

            CGPROGRAM
            #include "UnityCustomRenderTexture.cginc"
            #include "UnityCG.cginc"
            #include "Includes/PerlinNoise2D.cginc"
            #pragma vertex CustomRenderTextureVertexShader
            #pragma fragment frag
            #pragma target 3.0

            sampler2D _WetnessMap;
            float3 _LeftHandPos;
            float3 _RightHandPos;
            float _WaterActive;
            float _Darkness;
            float _Range;
            float _FadeSpeed;
            float _NoiseScale;
            float3 _WorldOrigin;
            float4 _WorldScale;
            float _WorldRotationY;

            float4 frag(v2f_customrendertexture IN) : SV_Target
            {
                float2 uv = IN.localTexcoord.xy;
                float wetnessFactor = tex2D(_WetnessMap, uv).r;

                // Estimate the world position
                float localX = -(uv.x - 0.5) * _WorldScale.x;
                float localZ = -(uv.y - 0.5) * _WorldScale.z;
                float3 positionWS = float3(_WorldOrigin.x + localX, _WorldOrigin.y, _WorldOrigin.z + localZ);
                
                float dt = unity_DeltaTime.x;
                wetnessFactor = max(wetnessFactor - _FadeSpeed * dt, 0.0);

                if (_WaterActive > 0.0)
                {
                    float3 pointA = _LeftHandPos;
                    float3 pointB = _RightHandPos;

                    float distA = distance(positionWS.xz, pointA.xz);
                    float distB = distance(positionWS.xz, pointB.xz);

                    float proximityFactorA = (1.0 - saturate(distA / _Range)) * _Darkness;
                    float proximityFactorB = (1.0 - saturate(distB / _Range)) * _Darkness;
                    float newWetness = max(proximityFactorA, proximityFactorB);

                    float edgeFactor = (1.0 - saturate(min(distA, distB) / _Range)) * 0.5;
                    float splatterWetness = lerp(newWetness * cloudNoise(uv * _NoiseScale), newWetness, edgeFactor);

                    wetnessFactor += splatterWetness;
                }

                wetnessFactor = clamp(wetnessFactor, 0, 1);
                return float4(wetnessFactor, wetnessFactor, wetnessFactor, 1.0);
            }
            ENDCG
        }
    }
}
