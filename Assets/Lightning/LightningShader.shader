Shader "Custom/LightningShader"
{
    Properties
    {
        _SourceColor("Source Color", Color) = (1, 0, 0, 1)
        _TargetColor("Target Color", Color) = (0, 0, 1, 1)
        _Width("Width", Float) = 0.1
        _SourcePoint("Source Point", Vector) = (0, 0, 0, 1)
        _TargetPoint("Target Point", Vector) = (0, 0, 0, 1)
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
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
                uint vertexIndex : SV_VertexID;
            };

            // Vertex to Geometry structure
            struct v2g
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            // // Hull Shader Output (Control Points)
            // struct hs
            // {
            //     float3 cpoint : CPOINT;
            // };

            // // Hull Shader Patch Constant Output
            // struct tf
            // {
            //     float tessellationFactor : SV_TessFactor; // Main tessellation factor
            //     float tessFactorInside : SV_InsideTessFactor; // Inside tessellation factor
            // };

            // // Domain Shader Output
            // struct ds
            // {
            //     float4 position : SV_Position;
            //     float3 worldPos : TEXCOORD0;
            // };

            // Geometry to Fragment structure
            struct g2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float2 uv : TEXCOORD1;
            };

            float4 _SourcePoint;
            float4 _TargetPoint;
            float4 _SourceColor;
            float4 _TargetColor;
            float _Width;

            // Vertex Shader
            v2g vert(appdata_t v)
            {
                v2g o;
                float4 worldPosition = (v.vertexIndex == 0) ? _SourcePoint : _TargetPoint;
                o.pos = UnityObjectToClipPos(worldPosition);
                o.worldPos = worldPosition.xyz;
                return o;
            }

            // // TESSELATION

            // // Hull Shader
            // [UNITY_domain("isoline")]
            // [UNITY_outputcontrolpoints(2)]
            // [UNITY_partitioning("integer")]
            // [UNITY_outputtopology("line")] // could also try point
            // [UNITY_patchconstantfunc("hullTessellationFactors")]
            // hs hullProgram(InputPatch<v2g, 2> patch, uint id : SV_OutputControlPointID)
            // {
            //     hs output;
            //     output.cpoint = patch[id].worldPos;
            //     return output;
            // }

            // tf hullTessellationFactors(InputPatch<v2g, 2> patch, uint patchID : SV_PrimitiveID)
            // {
            //     tf factors;
            //     factors.tessellationFactor = 8.0; // Number of segments along the isoline
            //     factors.tessFactorInside = 1.0; // Orthogonal tessellation (always 1 for isoline)
            //     return factors;
            // }
            
            // // Domain Shader
            // [UNITY_domain("isoline")]
            // ds domainShader(tf input, OutputPatch<hs, 2> op, float2 uv : SV_DomainLocation)
            // {
            //     ds output;

            //     // Interpolate the position between the two control points based on uv.x
            //     float3 interpolatedPos = lerp(op[0].worldPos, op[1].worldPos, uv.x);

            //     // Set the position and world position
            //     output.position = UnityObjectToClipPos(float4(interpolatedPos, 1.0));
            //     output.worldPos = interpolatedPos;

            //     return output;
            // }


            // TESSELATION


            // Geometry Shader
            [maxvertexcount(6)]
            void geom(line v2g input[2], inout TriangleStream<g2f> stream)
            {
                g2f o;

                float3 edgeDir = normalize(input[1].worldPos - input[0].worldPos);
                float3 cameraDir = normalize(_WorldSpaceCameraPos - input[0].worldPos);
                float3 billboardDir = normalize(cross(edgeDir, cameraDir));

                float3 offset = billboardDir * _Width * 0.5;

                float3 vertices[4] = {
                    input[0].worldPos - offset,
                    input[0].worldPos + offset,
                    input[1].worldPos - offset,
                    input[1].worldPos + offset
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
            }

            // Fragment Shader
            fixed4 frag(g2f i) : SV_Target
            {
                return lerp(_SourceColor, _TargetColor, i.uv.x);
            }
            ENDCG
        }
    }
}
