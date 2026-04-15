Shader "Showcase/SSS"
{
    Properties
    {
        _BaseColor      ("Base Color", Color) = (0.85, 0.55, 0.45, 1)
        _SSSColor       ("SSS Color", Color) = (1, 0.3, 0.15, 1)
        _SSSPower       ("SSS Power", Range(1, 16)) = 4
        _SSSStrength    ("SSS Strength", Range(0, 3)) = 1.2
        _SSSDistortion  ("SSS Distortion", Range(0, 1)) = 0.3
        _Thickness      ("Thickness", Range(0, 1)) = 0.5
        _Ambient        ("Ambient", Range(0, 0.5)) = 0.15
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        Pass
        {
            Name "SSS"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            float4 _BaseColor;
            float4 _SSSColor;
            float _SSSPower;
            float _SSSStrength;
            float _SSSDistortion;
            float _Thickness;
            float _Ambient;

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

                // 主光源
                Light mainLight = GetMainLight();
                float3 L = normalize(mainLight.direction);

                // === 正面光照（Lambert） ===
                float NdotL = saturate(dot(N, L));
                half3 diffuse = _BaseColor.rgb * NdotL * mainLight.color;

                // === 次表面散射 ===
                // 光從背面穿透：用 -L + N*distortion 模擬散射方向
                float3 sssDir = L + N * _SSSDistortion;
                float VdotSSS = saturate(dot(V, -sssDir));

                // 穿透強度：視角對準光源背面時最強
                float sss = pow(VdotSSS, _SSSPower) * _SSSStrength;

                // 厚度控制：薄的地方穿透多（用法線朝向模擬）
                float thicknessFactor = saturate(1.0 - dot(N, V) * (1.0 - _Thickness));
                sss *= thicknessFactor;

                half3 sssColor = _SSSColor.rgb * sss * mainLight.color;

                // === 邊緣光（Rim）增強透光邊緣 ===
                float rim = pow(1.0 - saturate(dot(N, V)), 3.0);
                half3 rimColor = _SSSColor.rgb * rim * 0.4;

                // === 合成 ===
                half3 ambient = _BaseColor.rgb * _Ambient;
                half3 finalColor = diffuse + sssColor + rimColor + ambient;

                return half4(finalColor, 1);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
