Shader "Showcase/RimLight"
{
    Properties
    {
        _BaseColor  ("Base Color", Color) = (0.2, 0.2, 0.25, 1)
        _RimColor   ("Rim Color", Color) = (0.3, 0.6, 1, 1)
        _RimPower   ("Rim Power", Range(1, 8)) = 3
        _RimIntensity ("Rim Intensity", Range(0, 5)) = 1.5
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        Pass
        {
            Name "RimLight"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            float4 _BaseColor;
            float4 _RimColor;
            float _RimPower;
            float _RimIntensity;

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

                // 主光源 Lambert
                Light mainLight = GetMainLight();
                float3 L = normalize(mainLight.direction);
                float NdotL = saturate(dot(N, L));
                half3 diffuse = _BaseColor.rgb * (NdotL * 0.8 + 0.2) * mainLight.color;

                // ★ 核心：Rim Light 邊緣光
                // 法線和視角方向的點積，邊緣處接近 0
                float rim = 1.0 - saturate(dot(N, V));
                // pow 控制邊緣光的寬窄
                rim = pow(rim, _RimPower) * _RimIntensity;

                half3 rimColor = _RimColor.rgb * rim;

                half3 finalColor = diffuse + rimColor;
                return half4(finalColor, 1);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
