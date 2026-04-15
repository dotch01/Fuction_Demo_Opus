Shader "Showcase/Dithering"
{
    Properties
    {
        _BaseColor   ("Base Color", Color) = (0.3, 0.7, 0.4, 1)
        _FadeColor   ("Fade Color", Color) = (0.1, 0.1, 0.15, 1)
        _DitherScale ("Dither Scale", Range(1, 8)) = 1
        _Opacity     ("Opacity", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        Pass
        {
            Name "Dithering"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            float4 _BaseColor;
            float4 _FadeColor;
            float _DitherScale;
            float _Opacity;

            // ★ 核心：Bayer 4x4 抖動矩陣
            // 16 個閾值，規律排列產生有序抖動
            static const float bayerMatrix[16] = {
                 0.0/16.0,  8.0/16.0,  2.0/16.0, 10.0/16.0,
                12.0/16.0,  4.0/16.0, 14.0/16.0,  6.0/16.0,
                 3.0/16.0, 11.0/16.0,  1.0/16.0,  9.0/16.0,
                15.0/16.0,  7.0/16.0, 13.0/16.0,  5.0/16.0
            };

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 viewDirWS : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.viewDirWS = GetWorldSpaceNormalizeViewDir(output.positionWS);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float3 N = normalize(input.normalWS);
                float3 V = normalize(input.viewDirWS);

                // 基礎光照
                Light mainLight = GetMainLight();
                float3 L = normalize(mainLight.direction);
                float NdotL = saturate(dot(N, L));

                half3 litColor = _BaseColor.rgb * (NdotL * 0.8 + 0.2) * mainLight.color;

                // ★ 核心：根據像素螢幕位置查 Bayer 矩陣
                // 決定該像素要不要顯示 → 模擬半透明
                float2 screenPos = input.positionCS.xy / _DitherScale;
                int2 ditherCoord = int2(fmod(screenPos.x, 4.0), fmod(screenPos.y, 4.0));
                // 防止負數索引
                ditherCoord = abs(ditherCoord);
                int idx = ditherCoord.x + ditherCoord.y * 4;
                // Clamp 防止越界
                idx = clamp(idx, 0, 15);
                float threshold = bayerMatrix[idx];

                // 用 _Opacity 和 threshold 比較
                // 低於閾值就 clip 掉 → 產生抖動圖案
                clip(_Opacity - threshold);

                // 混合底色（被 clip 的像素露出背後色）
                half3 finalColor = litColor;
                return half4(finalColor, 1);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
