Shader "Showcase/Portal"
{
    Properties
    {
        _PortalTex ("Portal Texture", 2D) = "black" {}
        _EdgeColor ("Edge Color", Color) = (0.2, 0.6, 1, 1)
        _EdgeWidth ("Edge Width", Range(0.01, 0.15)) = 0.05
        _EdgeGlow  ("Edge Glow Intensity", Range(0, 5)) = 2.0
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        Stencil
        {
            Ref 1
            Comp Always
            Pass Replace
        }

        Pass
        {
            Name "Portal"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_PortalTex);
            SAMPLER(sampler_PortalTex);

            float4 _EdgeColor;
            float _EdgeWidth;
            float _EdgeGlow;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.screenPos = ComputeScreenPos(output.positionCS);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 screenUV = input.screenPos.xy / input.screenPos.w;
                half4 portalColor = SAMPLE_TEXTURE2D(_PortalTex, sampler_PortalTex, screenUV);

                // 圓形邊緣光暈
                float2 centeredUV = input.uv - 0.5;
                float dist = length(centeredUV) * 2.0;
                float edge = smoothstep(1.0 - _EdgeWidth * 2.0, 1.0, dist);

                // 邊緣脈動
                float pulse = sin(_Time.y * 3.0) * 0.15 + 0.85;
                half4 glowColor = _EdgeColor * _EdgeGlow * pulse;

                return lerp(portalColor, glowColor, edge);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
