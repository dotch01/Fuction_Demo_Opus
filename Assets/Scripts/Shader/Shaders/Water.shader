Shader "Showcase/Water"
{
    Properties
    {
        _ShallowColor ("Shallow Color", Color) = (0.3, 0.7, 0.9, 0.6)
        _DeepColor    ("Deep Color", Color) = (0.05, 0.15, 0.4, 0.9)
        _WaveHeight   ("Wave Height", Range(0, 0.5)) = 0.15
        _WaveSpeed    ("Wave Speed", Range(0, 5)) = 1.5
        _WaveFreq     ("Wave Frequency", Range(1, 20)) = 6
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" }

        Pass
        {
            Name "Water"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float4 _ShallowColor;
            float4 _DeepColor;
            float _WaveHeight;
            float _WaveSpeed;
            float _WaveFreq;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 viewDirWS : TEXCOORD2;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;

                float3 pos = input.positionOS.xyz;
                float t = _Time.y * _WaveSpeed;

                // 多層 sin 波疊加
                pos.y += sin(pos.x * _WaveFreq + t) * _WaveHeight * 0.5;
                pos.y += sin(pos.z * _WaveFreq * 0.7 + t * 1.3) * _WaveHeight * 0.3;
                pos.y += sin((pos.x + pos.z) * _WaveFreq * 0.5 + t * 0.8) * _WaveHeight * 0.2;

                output.positionCS = TransformObjectToHClip(pos);
                output.uv = input.uv;

                float3 worldPos = TransformObjectToWorld(pos);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.viewDirWS = GetWorldSpaceNormalizeViewDir(worldPos);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // 菲涅爾邊緣效果
                float fresnel = 1.0 - saturate(dot(normalize(input.normalWS), normalize(input.viewDirWS)));
                fresnel = pow(fresnel, 3.0);

                // UV 動畫模擬水流
                float2 flowUV = input.uv + _Time.y * float2(0.03, 0.02);
                float pattern = sin(flowUV.x * 30.0) * sin(flowUV.y * 30.0) * 0.1 + 0.9;

                half4 color = lerp(_ShallowColor, _DeepColor, fresnel);
                color.rgb *= pattern;
                color.rgb += fresnel * 0.3; // 邊緣高光

                return color;
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
