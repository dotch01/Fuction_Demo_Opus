Shader "Showcase/Bubble"
{
    Properties
    {
        _BaseColor    ("Base Color", Color) = (0.5, 0.8, 1, 0.15)
        _FresnelPower ("Fresnel Power", Range(1, 8)) = 3
        _RainbowStrength ("Rainbow Strength", Range(0, 2)) = 1
        _Thickness    ("Film Thickness", Range(0.5, 3)) = 1.5
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" }

        Pass
        {
            Name "Bubble"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float4 _BaseColor;
            float _FresnelPower;
            float _RainbowStrength;
            float _Thickness;

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
            };

            // 簡易彩虹色（薄膜干涉）
            half3 rainbow(float t)
            {
                half3 c;
                c.r = sin(t * 6.2831 * 1.0) * 0.5 + 0.5;
                c.g = sin(t * 6.2831 * 1.0 + 2.094) * 0.5 + 0.5;
                c.b = sin(t * 6.2831 * 1.0 + 4.188) * 0.5 + 0.5;
                return c;
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                float3 worldPos = TransformObjectToWorld(input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.viewDirWS = GetWorldSpaceNormalizeViewDir(worldPos);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float3 N = normalize(input.normalWS);
                float3 V = normalize(input.viewDirWS);

                // 菲涅爾：邊緣更亮
                float fresnel = pow(1.0 - saturate(dot(N, V)), _FresnelPower);

                // 薄膜干涉彩虹色
                float filmAngle = dot(N, V) * _Thickness + _Time.y * 0.3;
                half3 iridescence = rainbow(filmAngle) * _RainbowStrength;

                half3 color = _BaseColor.rgb + iridescence * fresnel;
                float alpha = _BaseColor.a + fresnel * 0.6;

                return half4(color, saturate(alpha));
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
