Shader "Showcase/Fire"
{
    Properties
    {
        _FireSpeed    ("Fire Speed", Range(0.1, 5)) = 2
        _Distortion   ("Distortion", Range(0, 2)) = 0.8
        _NoiseScale   ("Noise Scale", Range(1, 20)) = 6
        _Intensity    ("Intensity", Range(0.5, 3)) = 1.5
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" }

        Pass
        {
            Name "Fire"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha One
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float _FireSpeed;
            float _Distortion;
            float _NoiseScale;
            float _Intensity;

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

            // 簡易 2D 雜訊
            float hash(float2 p)
            {
                return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);

                float a = hash(i);
                float b = hash(i + float2(1, 0));
                float c = hash(i + float2(0, 1));
                float d = hash(i + float2(1, 1));

                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            float fbm(float2 p)
            {
                float v = 0.0;
                v += 0.500 * noise(p); p *= 2.01;
                v += 0.250 * noise(p); p *= 2.02;
                v += 0.125 * noise(p); p *= 2.03;
                v += 0.0625 * noise(p);
                return v;
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
                float2 uv = input.uv;

                // UV 向上滾動 + 雜訊扭曲
                float t = _Time.y * _FireSpeed;
                float2 noiseUV = uv * _NoiseScale;
                noiseUV.y -= t;

                float distortion = fbm(noiseUV * 0.5 + t * 0.3) * _Distortion;
                noiseUV.x += distortion;

                float n = fbm(noiseUV);

                // 頂部漸隱：越上面火焰越薄
                float heightFade = 1.0 - uv.y;
                n *= heightFade * heightFade;
                n *= _Intensity;

                // 火焰色彩漸層：白 → 黃 → 橙 → 紅 → 黑
                half3 col = half3(1, 1, 1);
                col = lerp(half3(1, 0.9, 0.2), half3(1, 1, 1), smoothstep(0.8, 1.2, n));
                col = lerp(half3(1, 0.4, 0.05), col, smoothstep(0.4, 0.8, n));
                col = lerp(half3(0.5, 0.05, 0.0), col, smoothstep(0.2, 0.45, n));
                col = lerp(half3(0, 0, 0), col, smoothstep(0.05, 0.2, n));

                float alpha = smoothstep(0.05, 0.3, n);
                return half4(col, alpha);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
