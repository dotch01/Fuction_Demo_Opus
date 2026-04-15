Shader "Showcase/ColorGrading"
{
    Properties
    {
        _MainTex    ("Texture", 2D) = "white" {}
        _GammaR     ("Gamma R", Range(0.2, 3)) = 1
        _GammaG     ("Gamma G", Range(0.2, 3)) = 1
        _GammaB     ("Gamma B", Range(0.2, 3)) = 1
        _Contrast   ("Contrast", Range(0.5, 2)) = 1
        _Saturation ("Saturation", Range(0, 2)) = 1
        _Brightness ("Brightness", Range(0.5, 2)) = 1
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        Pass
        {
            Name "ColorGrading"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float _GammaR;
            float _GammaG;
            float _GammaB;
            float _Contrast;
            float _Saturation;
            float _Brightness;

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

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half3 color = tex.rgb;

                // ★ 核心：Color Grading 調色

                // 1. Gamma 曲線 — 分別調整 RGB 三個通道
                color.r = pow(max(color.r, 0.001), _GammaR);
                color.g = pow(max(color.g, 0.001), _GammaG);
                color.b = pow(max(color.b, 0.001), _GammaB);

                // 2. 亮度
                color *= _Brightness;

                // 3. 對比度 — 以 0.5 為中心縮放
                color = (color - 0.5) * _Contrast + 0.5;

                // 4. 飽和度 — 用亮度做灰度混合
                float lum = dot(color, half3(0.299, 0.587, 0.114));
                color = lerp(half3(lum, lum, lum), color, _Saturation);

                return half4(saturate(color), 1);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
