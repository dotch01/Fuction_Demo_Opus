Shader "Showcase/ChromaticAberration"
{
    Properties
    {
        _MainTex   ("Texture", 2D) = "white" {}
        _Intensity ("Intensity", Range(0, 0.05)) = 0.01
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        Pass
        {
            Name "ChromaticAberration"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
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

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 dir = input.uv - 0.5;
                float dist = length(dir);
                float2 offset = normalize(dir) * _Intensity * dist;

                // 三次採樣：R 偏移, G 不偏移, B 反向偏移
                half r = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv + offset).r;
                half g = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv).g;
                half b = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv - offset).b;

                return half4(r, g, b, 1.0);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
