Shader "Custom/LightningBillboardShader"
{
    Properties
    {
        _SourceColor("Source Color", Color) = (1, 0, 0, 1)
        _TargetColor("Target Color", Color) = (0, 0, 1, 1)
        _Width("Width", Float) = 0.1
        _OffsetScale("Offset Scale", Float) = 0.3
        _RandSeed("Jitter Randomness Seed", Float) = 0
        
        _LightningTexture("Lightning Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            ZWrite On

            CGPROGRAM
            #pragma target 4.6
            #pragma vertex vert
            //#pragma hull hullProgram
            //#pragma domain domainShader
            #pragma geometry geom
            #pragma fragment frag
            #include "UnityCG.cginc"

            // Vertex input structure
            struct appdata_t
            {
                float4 pos : POSITION;
            };

            // Vertex to Geometry structure
            struct v2g
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            // Geometry to Fragment structure
            struct g2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float2 uv : TEXCOORD1;
            };

            float4 _SourceColor;
            float4 _TargetColor;
            float _OffsetScale;
            float _RandSeed;
            float _Width;
            sampler2D _LightningTexture;

            float Random(int seed)
            {
                // Use a large prime number and sine to generate randomness
                float random = frac(sin(seed * 12.9898) * 43758.5453123 * _RandSeed);
                return random;
            }

            v2g vert(appdata_t v)
            {
                v2g o;

                // Transform vertex position to clip space
                o.pos = UnityObjectToClipPos(v.pos);
                o.worldPos = mul(unity_ObjectToWorld, v.pos).xyz;

                return o;
            }

            // Geometry Shader
            [maxvertexcount(6)]
            void geom(line v2g input[2], inout TriangleStream<g2f> stream)
            {
                g2f o;

                float3 edgeDir = normalize(input[1].worldPos - input[0].worldPos);
                float3 cameraDir = normalize(_WorldSpaceCameraPos - input[0].worldPos);
                float3 billboardDir = normalize(cross(edgeDir, cameraDir));

                float3 offset = billboardDir * _Width * 0.5;

                float overShoot = _OffsetScale;

                float3 vertices[4] = {
                    input[0].worldPos - offset - edgeDir * overShoot,
                    input[0].worldPos + offset - edgeDir * overShoot,
                    input[1].worldPos - offset + edgeDir * overShoot,
                    input[1].worldPos + offset + edgeDir * overShoot
                };

                float2 uvs[4] = {
                    float2(0, 0),
                    float2(0, 1),
                    float2(1, 0),
                    float2(1, 1)
                };

                // Emit Triangle 1
                o.worldPos = vertices[0];
                o.uv = uvs[0];
                o.pos = UnityWorldToClipPos(float4(o.worldPos, 1.0));
                stream.Append(o);

                o.worldPos = vertices[1];
                o.uv = uvs[1];
                o.pos = UnityWorldToClipPos(float4(o.worldPos, 1.0));
                stream.Append(o);

                o.worldPos = vertices[2];
                o.uv = uvs[2];
                o.pos = UnityWorldToClipPos(float4(o.worldPos, 1.0));
                stream.Append(o);

                stream.RestartStrip();

                // Emit Triangle 2
                o.worldPos = vertices[2];
                o.uv = uvs[2];
                o.pos = UnityWorldToClipPos(float4(o.worldPos, 1.0));
                stream.Append(o);

                o.worldPos = vertices[1];
                o.uv = uvs[1];
                o.pos = UnityWorldToClipPos(float4(o.worldPos, 1.0));
                stream.Append(o);

                o.worldPos = vertices[3];
                o.uv = uvs[3];
                o.pos = UnityWorldToClipPos(float4(o.worldPos, 1.0));
                stream.Append(o);

                stream.RestartStrip();
            }

            // Fragment Shader
            // Fragment Shader
            fixed4 frag(g2f i) : SV_Target
            {
                // Interpolate color based on uv.x
                fixed4 col = _SourceColor;

                // // Calculate distance from center
                float distanceFromCenter = abs(i.uv.y - 0.5);

                // // Falloff for opacity
                float falloff = 1.0 - (distanceFromCenter * 4.0);
                falloff = saturate(falloff); // Clamp to [0, 1]

                // // Enhance brightness near center
                float brightnessBoost = 1.0 - smoothstep(0.0, 0.1, distanceFromCenter); // Smooth near center
                col.rgb = lerp(col.rgb, float3(1.0, 1.0, 1.0), brightnessBoost);

                // // falloff radially towards the ends of the uv.space
                float distanceFromCenterX = abs(i.uv.x - 0.5);
                falloff -= smoothstep(0.5, 1, distanceFromCenterX);

                // rotate uv coordinates 90 degrees and scale
                i.uv = float2(i.uv.y, clamp(i.uv.x * 0.2, 0, 1));

                // Sample lightning texture
                fixed4 texColor = tex2D(_LightningTexture, i.uv);
                col.a = texColor.a * falloff;

                // Alpha clipping
                clip(col.a < 0.2 ? -1:1);

                return col;
            }
            ENDCG
        }
    }
}
