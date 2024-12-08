Shader "Custom/LightningShader"
{
    Properties
    {
        _SourceColor("Source Color", Color) = (1, 0, 0, 1)
        _TargetColor("Target Color", Color) = (0, 0, 1, 1)
        _Width("Width", Float) = 0.1
        _SourcePoint("Source Point", Vector) = (0, 0, 0, 1)
        _TargetPoint("Target Point", Vector) = (0, 0, 0, 1)
        _OffsetScale("Offset Scale", Float) = 0.3
        _RandSeed("Jitter Randomness Seed", Float) = 0
        _VertexCount("Vertex Count", Integer) = 0
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
            float _OffsetScale;
            float _RandSeed;
            float _Width;
            int _VertexCount;

            float Random(int seed)
            {
                // Use a large prime number and sine to generate randomness
                float random = frac(sin(seed * 12.9898) * 43758.5453123 * _RandSeed);
                return random;
            }

            // Vertex Shader
            v2g vert(appdata_t v)
            {
                v2g o;

                // Interpolation factor
                float t = (float)v.vertexIndex / (float)_VertexCount;

                // Interpolated world position
                float4 worldPosition = lerp(_SourcePoint, _TargetPoint, t);

                // Direction vector from source to target
                float3 sourceToTargetVector = normalize(_TargetPoint.xyz - _SourcePoint.xyz);

                // Generate a random angle in degrees
                float randAngle = Random(v.vertexIndex) * 360.0;

                // Create a perpendicular direction vector
                float3 up = float3(0, 1, 0);
                if (abs(dot(up, sourceToTargetVector)) > 0.99) // Handle degenerate case
                    up = float3(1, 0, 0);
                float3 perpendicularVector = normalize(cross(sourceToTargetVector, up));

                // Rotate the perpendicular vector by a random angle
                float3 offsetDirection = normalize(
                    cos(radians(randAngle)) * perpendicularVector +
                    sin(radians(randAngle)) * cross(sourceToTargetVector, perpendicularVector)
                );

                // Ensure offset direction is not perpendicular to the camera view
                float3 cameraViewDirection = normalize(_WorldSpaceCameraPos.xyz - worldPosition.xyz);
                float alignment = dot(offsetDirection, cameraViewDirection);

                // Adjust offset direction if alignment is close to zero
                if (abs(alignment) < 0.1) // Threshold for near-perpendicularity
                {
                    offsetDirection = normalize(offsetDirection + cameraViewDirection * 0.5);
                }

                // Apply a random magnitude to the offset
                float offsetMagnitude = Random(v.vertexIndex + 42) * _OffsetScale * _RandSeed;
                float3 offset = offsetDirection * offsetMagnitude;

                // Add the offset to the world position
                worldPosition.xyz += offset;

                // Transform to clip space
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

                float overShoot = 0.01;

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
                fixed4 col = lerp(_SourceColor, _TargetColor, i.uv.x);

                // Calculate distance from center
                float distanceFromCenter = abs(i.uv.y - 0.5);

                // Falloff for opacity
                float falloff = 1.0 - (distanceFromCenter * 4.0);
                falloff = saturate(falloff); // Clamp to [0, 1]

                // Enhance brightness near center
                float brightnessBoost = 1.0 - smoothstep(0.0, 0.1, distanceFromCenter); // Smooth near center
                col.rgb = lerp(col.rgb, float3(1.0, 1.0, 1.0), brightnessBoost);

                // falloff radially towards the ends of the uv.space
                float distanceFromCenterX = abs(i.uv.x - 0.5);
                falloff -= smoothstep(0.5, 1, distanceFromCenterX);

                // Apply falloff to alpha
                col.a *= falloff;

                // Alpha clipping
                clip(col.a < 0.2 ? -1:1);

                return col;
            }
            ENDCG
        }
    }
}
