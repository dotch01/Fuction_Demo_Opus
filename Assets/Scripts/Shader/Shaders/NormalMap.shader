Shader "Showcase/NormalMap"
{
    Properties
    {
        _BaseColor       ("Base Color", Color) = (0.75, 0.65, 0.55, 1)
        _NormalStrength  ("Normal Strength", Range(0, 3)) = 1
        _BrickScale      ("Brick Scale", Range(1, 10)) = 4
        _LightAngle      ("Light Angle", Range(0, 360)) = 45
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        Pass
        {
            Name "NormalMap"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float4 _BaseColor;
            float _NormalStrength;
            float _BrickScale;
            float _LightAngle;

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
                float3 normalWS : TEXCOORD1;
                float3 tangentWS : TEXCOORD2;
                float3 bitangentWS : TEXCOORD3;
            };

            // ★ 程序化磚牆法線
            // 不需要外部貼圖，純數學產生凹凸
            float3 proceduralBrickNormal(float2 uv)
            {
                float2 brickUV = uv * _BrickScale;

                // 交錯排列：偶數列偏移半磚
                float row = floor(brickUV.y);
                float isOddRow = fmod(row, 2.0);  // use fmod instead of %
                brickUV.x += isOddRow * 0.5;

                // 磚塊內部座標 (0~1)
                float2 f = frac(brickUV);

                // 灰縫寬度
                float mortarW = 0.08;

                // 計算法線偏移：灰縫處法線向下凹
                float2 normalXY = float2(0, 0);

                // X 方向灰縫
                float edgeX = smoothstep(0.0, mortarW, f.x) * (1.0 - smoothstep(1.0 - mortarW, 1.0, f.x));
                // Y 方向灰縫
                float edgeY = smoothstep(0.0, mortarW, f.y) * (1.0 - smoothstep(1.0 - mortarW, 1.0, f.y));

                // 用微分近似法線
                float eps = 0.01;
                float2 brickUV_dx = (uv + float2(eps, 0)) * _BrickScale;
                brickUV_dx.x += fmod(floor(brickUV_dx.y), 2.0) * 0.5;
                float2 f_dx = frac(brickUV_dx);
                float h = edgeX * edgeY;
                float h_dx = smoothstep(0.0, mortarW, f_dx.x) * (1.0 - smoothstep(1.0 - mortarW, 1.0, f_dx.x))
                           * smoothstep(0.0, mortarW, f_dx.y) * (1.0 - smoothstep(1.0 - mortarW, 1.0, f_dx.y));

                float2 brickUV_dy = (uv + float2(0, eps)) * _BrickScale;
                brickUV_dy.x += fmod(floor(brickUV_dy.y), 2.0) * 0.5;
                float2 f_dy = frac(brickUV_dy);
                float h_dy = smoothstep(0.0, mortarW, f_dy.x) * (1.0 - smoothstep(1.0 - mortarW, 1.0, f_dy.x))
                           * smoothstep(0.0, mortarW, f_dy.y) * (1.0 - smoothstep(1.0 - mortarW, 1.0, f_dy.y));

                normalXY.x = (h - h_dx) / eps;
                normalXY.y = (h - h_dy) / eps;

                return normalize(float3(normalXY * _NormalStrength, 1.0));
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);

                float3 tangent = TransformObjectToWorldDir(input.tangentOS.xyz);
                output.tangentWS = tangent;
                output.bitangentWS = cross(output.normalWS, tangent) * input.tangentOS.w;

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float3 N = normalize(input.normalWS);
                float3 T = normalize(input.tangentWS);
                float3 B = normalize(input.bitangentWS);

                // 程序化法線（切線空間）
                float3 normalTS = proceduralBrickNormal(input.uv);

                // ★ 核心：TBN 矩陣將切線空間法線轉到世界空間
                // 讓平面看起來有凹凸，但模型面數完全沒變
                float3 normalWS = normalize(T * normalTS.x + B * normalTS.y + N * normalTS.z);

                // 用可控角度的光源
                float angleRad = _LightAngle * 3.14159 / 180.0;
                float3 L = normalize(float3(cos(angleRad), 0.6, sin(angleRad)));

                float NdotL = saturate(dot(normalWS, L));
                half3 color = _BaseColor.rgb * (NdotL * 0.85 + 0.15);

                return half4(color, 1);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
