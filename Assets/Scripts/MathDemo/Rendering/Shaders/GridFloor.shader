Shader "MathDemo/GridFloor"
{
    Properties
    {
        _BaseColor   ("Base Color", Color) = (0.12, 0.12, 0.15, 1)
        _LineColor   ("Line Color", Color) = (0.25, 0.25, 0.3, 1)
        _MajorColor  ("Major Line Color", Color) = (0.35, 0.35, 0.4, 1)
        _Spacing     ("Grid Spacing", Float) = 1.0
        _MajorEvery  ("Major Every N", Float) = 5.0
        _LineWidth   ("Line Width", Range(0.005, 0.05)) = 0.02
        _FadeStart   ("Fade Start Dist", Float) = 30
        _FadeEnd     ("Fade End Dist", Float) = 60
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        Pass
        {
            Name "GridFloor"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float4 _BaseColor;
            float4 _LineColor;
            float4 _MajorColor;
            float _Spacing;
            float _MajorEvery;
            float _LineWidth;
            float _FadeStart;
            float _FadeEnd;

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 worldXZ = input.positionWS.xz;

                // 小格線
                float2 gridUV = abs(frac(worldXZ / _Spacing + 0.5) - 0.5) * _Spacing;
                float2 ddUV = fwidth(worldXZ);
                float2 grid = smoothstep(_LineWidth + ddUV, ddUV * 0.5, gridUV);
                float gridLine = max(grid.x, grid.y);

                // 大格線
                float2 majorUV = abs(frac(worldXZ / (_Spacing * _MajorEvery) + 0.5) - 0.5) * (_Spacing * _MajorEvery);
                float2 majorGrid = smoothstep(_LineWidth * 2.0 + ddUV, ddUV * 0.5, majorUV);
                float majorLine = max(majorGrid.x, majorGrid.y);

                // XZ 軸線（紅 X、藍 Z）
                float axisX = smoothstep(_LineWidth * 3.0 + ddUV.y, ddUV.y * 0.5, abs(worldXZ.y));
                float axisZ = smoothstep(_LineWidth * 3.0 + ddUV.x, ddUV.x * 0.5, abs(worldXZ.x));

                // 距離淡出
                float dist = length(input.positionWS.xz - _WorldSpaceCameraPos.xz);
                float fade = 1.0 - saturate((dist - _FadeStart) / (_FadeEnd - _FadeStart));

                // 合成
                half3 color = _BaseColor.rgb;
                color = lerp(color, _LineColor.rgb, gridLine * fade);
                color = lerp(color, _MajorColor.rgb, majorLine * fade);

                // X 軸紅色、Z 軸藍色
                color = lerp(color, half3(0.7, 0.15, 0.15), axisX * fade);
                color = lerp(color, half3(0.15, 0.15, 0.7), axisZ * fade);

                return half4(color, 1);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
