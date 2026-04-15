Shader "Showcase/VoronoiNoise"
{
    Properties
    {
        _CellDensity ("Cell Density", Range(1, 20)) = 6
        _Speed       ("Speed", Range(0, 3)) = 1
        _Color1      ("Color Inside", Color) = (0.1, 0.1, 0.3, 1)
        _Color2      ("Color Edge", Color) = (0.4, 0.8, 1, 1)
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        Pass
        {
            Name "VoronoiNoise"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float _CellDensity;
            float _Speed;
            float4 _Color1;
            float4 _Color2;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float2 voronoiHash(float2 p)
            {
                p = float2(dot(p, float2(127.1, 311.7)),
                           dot(p, float2(269.5, 183.3)));
                return frac(sin(p) * 43758.5453);
            }

            // Voronoi 距離場
            float voronoi(float2 uv)
            {
                float2 g = floor(uv);
                float2 f = frac(uv);
                float minDist = 1.0;

                [unroll]
                for (int j = -1; j <= 1; j++)
                {
                    [unroll]
                    for (int i = -1; i <= 1; i++)
                    {
                        float2 neighbor = float2((float)i, (float)j);
                        float2 pt = voronoiHash(g + neighbor);
                        // 動畫：cell 中心隨時間移動
                        pt = 0.5 + 0.5 * sin(6.2831 * pt + _Time.y * _Speed);
                        float d = length(neighbor + pt - f);
                        minDist = min(minDist, d);
                    }
                }
                return minDist;
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv * _CellDensity;
                float dist = voronoi(uv);
                half4 color = lerp(_Color1, _Color2, dist);
                return color;
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
