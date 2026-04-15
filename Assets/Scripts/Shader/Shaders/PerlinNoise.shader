Shader "Showcase/PerlinNoise"
{
    Properties
    {
        _Scale    ("Scale", Range(1, 20)) = 5
        _Speed    ("Speed", Range(0, 3)) = 0.5
        _Octaves  ("FBM Octaves", Range(1, 6)) = 4
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        Pass
        {
            Name "PerlinNoise"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float _Scale;
            float _Speed;
            float _Octaves;

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

            // 梯度雜訊用的 hash
            float2 gradientHash(float2 p)
            {
                p = float2(dot(p, float2(127.1, 311.7)),
                           dot(p, float2(269.5, 183.3)));
                return -1.0 + 2.0 * frac(sin(p) * 43758.5453);
            }

            // Perlin 梯度雜訊
            float perlin(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f); // smoothstep

                return lerp(
                    lerp(dot(gradientHash(i + float2(0, 0)), f - float2(0, 0)),
                         dot(gradientHash(i + float2(1, 0)), f - float2(1, 0)), u.x),
                    lerp(dot(gradientHash(i + float2(0, 1)), f - float2(0, 1)),
                         dot(gradientHash(i + float2(1, 1)), f - float2(1, 1)), u.x),
                    u.y) * 0.5 + 0.5;
            }

            // FBM (Fractal Brownian Motion) 疊加多層 Perlin
            float fbm(float2 p, int octaves)
            {
                float value = 0.0;
                float amplitude = 0.5;
                float frequency = 1.0;

                for (int i = 0; i < octaves; i++)
                {
                    value += amplitude * perlin(p * frequency);
                    frequency *= 2.0;
                    amplitude *= 0.5;
                }
                return value;
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
                float2 p = input.uv * _Scale + _Time.y * _Speed;
                float n = fbm(p, (int)_Octaves);
                return half4(n, n, n, 1.0);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
