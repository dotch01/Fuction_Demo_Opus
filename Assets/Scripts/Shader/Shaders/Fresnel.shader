Shader "Showcase/Fresnel"
{
    Properties
    {
        _BaseColor      ("Base Color", Color) = (0.1, 0.15, 0.2, 0.15)
        _FresnelColor   ("Fresnel Color", Color) = (0.4, 0.7, 1, 1)
        _FresnelPower   ("Fresnel Power", Range(1, 8)) = 3
        _Opacity        ("Opacity", Range(0, 1)) = 0.3
        _EnvReflect     ("Env Reflection", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" }

        Pass
        {
            Name "Fresnel"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            float4 _BaseColor;
            float4 _FresnelColor;
            float _FresnelPower;
            float _Opacity;
            float _EnvReflect;

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
                float NdotL = saturate(dot(N, L));

                // ★ 核心：Fresnel — 越斜著看越反光
                // 物理上真實存在的現象，玻璃、水面都是這樣
                float fresnel = pow(1.0 - saturate(dot(N, V)), _FresnelPower);

                // 模擬環境反射（用法線方向的假反射色）
                half3 fakeEnv = half3(0.3, 0.4, 0.6) + N.y * 0.2;
                half3 envColor = fakeEnv * _EnvReflect;

                // 基礎透明色 + 菲涅爾反射色
                half3 baseCol = _BaseColor.rgb * (NdotL * 0.5 + 0.5) * mainLight.color;
                half3 fresnelCol = _FresnelColor.rgb * fresnel;

                half3 finalColor = lerp(baseCol, envColor + fresnelCol, fresnel);

                // 透明度：正面透、邊緣不透
                float alpha = lerp(_Opacity, 1.0, fresnel * 0.8);

                return half4(finalColor, saturate(alpha));
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
