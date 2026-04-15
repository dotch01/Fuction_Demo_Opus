Shader "Showcase/Glow"
{
    Properties
    {
        _GlowColor    ("Glow Color", Color) = (0.2, 0.6, 1, 1)
        _Intensity    ("Intensity", Range(0.5, 10)) = 3
        _Softness     ("Softness", Range(0.1, 2)) = 0.8
        _PulseSpeed   ("Pulse Speed", Range(0, 5)) = 2
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" }

        Pass
        {
            Name "Glow"
            Tags { "LightMode" = "UniversalForward" }

            // Additive 疊加發光
            Blend One One
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float4 _GlowColor;
            float _Intensity;
            float _Softness;
            float _PulseSpeed;

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
                float2 centered = input.uv - 0.5;
                float dist = length(centered) * 2.0;

                // 軟邊光暈：距離衰減
                float glow = exp(-dist * dist / (_Softness * _Softness));

                // 脈動
                float pulse = sin(_Time.y * _PulseSpeed) * 0.2 + 0.8;

                half3 color = _GlowColor.rgb * glow * _Intensity * pulse;
                return half4(color, 0);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
