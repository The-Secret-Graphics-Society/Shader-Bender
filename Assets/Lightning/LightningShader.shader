Shader "Custom/LightningShader"
{
    Properties
    {
        _SourceColor("Source Color", Color) = (1, 0, 0, 1)
        _TargetColor("Target Color", Color) = (0, 0, 1, 1)
        _Width("Width", Float) = 0.1
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag
            #include "UnityCG.cginc"

            // Vertex input structure
            struct appdata_t
            {
                float4 vertex : POSITION;
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

            float4 _SourcePoint;
            float4 _TargetPoint;
            float4 _SourceColor;
            float4 _TargetColor;
            float _Width;

            // Vertex Shader
            v2g vert(appdata_t v)
            {
                v2g o;

                // Transform vertex to clip space
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

                return o;
            }

            // Geometry Shader
            [maxvertexcount(6)]
            void geom(line v2g input[2], inout TriangleStream<g2f> stream)
            {
                g2f o;

                // Compute directions
                float3 edgeDir = normalize(input[1].worldPos - input[0].worldPos);
                float3 cameraDir = normalize(_WorldSpaceCameraPos - input[0].worldPos);
                float3 billboardDir = normalize(cross(edgeDir, cameraDir));

                // Compute offsets
                float3 offset = billboardDir * _Width * 0.5;

                // Generate two triangles for the billboard
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

                // Triangle 1
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

                // Triangle 2
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
            fixed4 frag(g2f i) : SV_Target
            {
                // Interpolate color along the edge
                return lerp(_SourceColor, _TargetColor, i.uv.x);
            }
            ENDCG
        }
    }
}
