Shader "Showcase/Fog"
{
    Properties
    {
        _FogColor   ("Fog Color", Color) = (0.6, 0.7, 0.8, 1)
        _Density    ("Density", Range(0, 5)) = 1.5
        _Height     ("Fog Height", Range(0, 10)) = 3
        _Speed      ("Drift Speed", Range(0, 2)) = 0.3
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" }

        Pass
        {
            Name "Fog"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float4 _FogColor;
            float _Density;
            float _Height;
            float _Speed;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
            };

            float hash(float2 p)
            {
                return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                return lerp(
                    lerp(hash(i), hash(i + float2(1, 0)), f.x),
                    lerp(hash(i + float2(0, 1)), hash(i + float2(1, 1)), f.x), f.y);
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.worldPos = TransformObjectToWorld(input.positionOS.xyz);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // 霧的密度基於高度漸變
                float heightFactor = saturate(1.0 - input.uv.y / _Height * 2.0);
                heightFactor = heightFactor * heightFactor;

                // 雜訊讓霧更自然
                float2 driftUV = input.uv * 3.0 + float2(_Time.y * _Speed, 0);
                float n = noise(driftUV) * 0.5 + noise(driftUV * 2.3 + 17.0) * 0.3;

                float fogDensity = heightFactor * n * _Density;
                fogDensity = saturate(fogDensity);

                return half4(_FogColor.rgb, fogDensity);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
