Shader "Showcase/POM"
{
    Properties
    {
        _BaseColor    ("Base Color", Color) = (0.7, 0.65, 0.55, 1)
        _GrooveColor  ("Groove Color", Color) = (0.25, 0.2, 0.15, 1)
        _HeightScale  ("Height Scale", Range(0, 0.3)) = 0.1
        _Steps        ("Steps", Range(4, 64)) = 32
        _BrickScale   ("Brick Scale", Range(1, 10)) = 4
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        Pass
        {
            Name "POM"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float4 _BaseColor;
            float4 _GrooveColor;
            float _HeightScale;
            float _Steps;
            float _BrickScale;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 tangentViewDir : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float3 positionWS : TEXCOORD3;
            };

            // 程序化磚牆高度圖
            float brickHeight(float2 uv)
            {
                float2 brickUV = uv * _BrickScale;

                // 奇數列偏移半磚
                float row = floor(brickUV.y);
                float offset = step(1.0, fmod(row, 2.0)) * 0.5;
                brickUV.x += offset;

                // 磚的邊框
                float2 brickFrac = frac(brickUV);
                float mortarW = 0.06;
                float bx = smoothstep(0.0, mortarW, brickFrac.x) * smoothstep(0.0, mortarW, 1.0 - brickFrac.x);
                float by = smoothstep(0.0, mortarW, brickFrac.y) * smoothstep(0.0, mortarW, 1.0 - brickFrac.y);
                float h = bx * by;

                // 磚面微小凹凸
                float detail = frac(sin(dot(floor(brickUV), float2(12.9898, 78.233))) * 43758.5453);
                h *= 0.85 + detail * 0.15;

                return h;
            }

            // 程序化法線（中心差分）
            float3 brickNormal(float2 uv, float eps)
            {
                float hL = brickHeight(uv - float2(eps, 0));
                float hR = brickHeight(uv + float2(eps, 0));
                float hD = brickHeight(uv - float2(0, eps));
                float hU = brickHeight(uv + float2(0, eps));
                float3 n;
                n.x = (hL - hR);
                n.y = (hD - hU);
                n.z = eps * 2.0;
                return normalize(n);
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);

                // 計算切線空間的視線方向
                float3 worldTangent = TransformObjectToWorldDir(input.tangentOS.xyz);
                float3 worldBitangent = cross(output.normalWS, worldTangent) * input.tangentOS.w;
                float3 viewDirWS = GetWorldSpaceNormalizeViewDir(output.positionWS);

                // 轉到切線空間
                output.tangentViewDir = float3(
                    dot(viewDirWS, worldTangent),
                    dot(viewDirWS, worldBitangent),
                    dot(viewDirWS, output.normalWS)
                );

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // === Parallax Occlusion Mapping ===
                float3 viewDir = normalize(input.tangentViewDir);

                // 防止掠角時步進過長
                float parallaxLimit = -length(viewDir.xy) / max(viewDir.z, 0.001);

                float2 offsetDir = normalize(viewDir.xy);
                float2 maxOffset = offsetDir * parallaxLimit * _HeightScale;

                int numSteps = (int)_Steps;
                float stepSize = 1.0 / (float)numSteps;

                float2 dx = ddx(input.uv);
                float2 dy = ddy(input.uv);

                float currentRayHeight = 1.0;
                float2 currentOffset = float2(0, 0);
                float currentSample = brickHeight(input.uv);

                float prevSample = currentSample;
                float2 prevOffset = currentOffset;

                int stepIndex = 0;

                // Ray March：逐步下降找交點
                [loop]
                for (int si = 0; si < 64; si++)
                {
                    if (si >= numSteps) break;
                    if (currentRayHeight <= currentSample) break;

                    prevSample = currentSample;
                    prevOffset = currentOffset;

                    currentOffset += stepSize * maxOffset;
                    currentRayHeight -= stepSize;
                    currentSample = brickHeight(input.uv + currentOffset);
                }

                // 二分法精修交點
                float prevRayHeight = currentRayHeight + stepSize;
                float t = (prevRayHeight - prevSample) /
                          (prevRayHeight - prevSample + currentSample - currentRayHeight);
                float2 finalOffset = lerp(prevOffset, currentOffset, t);

                float2 finalUV = input.uv + finalOffset;

                // === 著色 ===
                float h = brickHeight(finalUV);
                float3 localNormal = brickNormal(finalUV, 0.002);

                // 光照
                float3 lightDir = normalize(float3(0.6, 1.0, 0.4));
                float NdotL = saturate(dot(localNormal, lightDir)) * 0.65 + 0.35;

                // 自遮擋陰影（簡易）
                float shadow = 1.0;
                float2 shadowOffset = float2(0, 0);
                float shadowRayHeight = h;
                float2 shadowDir = normalize(lightDir.xy) * _HeightScale * 0.5;
                for (int sj = 0; sj < 8; sj++)
                {
                    shadowOffset += shadowDir * 0.125;
                    shadowRayHeight += 0.125;
                    float shadowSample = brickHeight(finalUV + shadowOffset);
                    if (shadowSample > shadowRayHeight)
                    {
                        shadow = 0.6;
                        break;
                    }
                }

                half3 color = lerp(_GrooveColor.rgb, _BaseColor.rgb, h);
                color *= NdotL * shadow;

                return half4(color, 1);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
