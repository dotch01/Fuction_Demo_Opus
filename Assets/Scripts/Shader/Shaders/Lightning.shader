Shader "Showcase/Lightning"
{
    Properties
    {
        _Color      ("Lightning Color", Color) = (0.6, 0.8, 1, 1)
        _GlowColor  ("Glow Color", Color) = (0.3, 0.5, 1, 1)
        _Thickness   ("Thickness", Range(0.001, 0.1)) = 0.02
        _Branches    ("Branches", Range(0, 5)) = 2
        _FlickerSpeed("Flicker Speed", Range(1, 20)) = 8
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" }

        Pass
        {
            Name "Lightning"
            Tags { "LightMode" = "UniversalForward" }

            Blend One One
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float4 _Color;
            float4 _GlowColor;
            float _Thickness;
            float _Branches;
            float _FlickerSpeed;

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

            float hash(float p)
            {
                return frac(sin(p * 127.1) * 43758.5453);
            }

            float noise1D(float p)
            {
                float i = floor(p);
                float f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                return lerp(hash(i), hash(i + 1.0), f);
            }

            // 單條閃電線
            float lightning(float2 uv, float seed, float thickness)
            {
                float offset = 0.0;
                float amplitude = 0.3;

                // 多層雜訊累加產生鋸齒路徑
                for (int i = 0; i < 5; i++)
                {
                    offset += (noise1D(uv.y * (3.0 + i * 2.0) + _Time.y * 5.0 + seed) - 0.5) * amplitude;
                    amplitude *= 0.5;
                }

                float dist = abs(uv.x - 0.5 - offset);

                // 核心亮線 + 光暈
                float core = exp(-dist / thickness);
                float glow = exp(-dist / (thickness * 5.0)) * 0.4;

                return core + glow;
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
                float intensity = 0.0;

                // 主閃電
                intensity += lightning(input.uv, 0.0, _Thickness);

                // 分支
                for (int i = 1; i <= (int)_Branches; i++)
                {
                    float2 branchUV = input.uv;
                    branchUV.x += (hash(float(i) * 13.7) - 0.5) * 0.3;
                    branchUV.y = branchUV.y * 0.6 + hash(float(i) * 7.3) * 0.4;
                    intensity += lightning(branchUV, float(i) * 17.0, _Thickness * 0.7) * 0.5;
                }

                // 閃爍
                float flicker = saturate(sin(_Time.y * _FlickerSpeed) * 0.5 + 0.7);
                flicker *= saturate(sin(_Time.y * _FlickerSpeed * 3.7) * 0.3 + 0.8);
                intensity *= flicker;

                half3 col = _Color.rgb * intensity + _GlowColor.rgb * intensity * 0.3;
                return half4(col, intensity);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
