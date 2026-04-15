Shader "Showcase/ClothFlag"
{
    Properties
    {
        _BaseColor    ("Base Color", Color) = (0.8, 0.15, 0.15, 1)
        _StripeColor  ("Stripe Color", Color) = (0.9, 0.8, 0.2, 1)
        _WindStrength ("Wind Strength", Range(0, 2)) = 0.8
        _WindSpeed    ("Wind Speed", Range(0, 10)) = 3
        _WindFreq     ("Wind Frequency", Range(1, 10)) = 3
        _FlagWave     ("Flag Wave", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        Pass
        {
            Name "ClothFlag"
            Tags { "LightMode" = "UniversalForward" }

            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            float4 _BaseColor;
            float4 _StripeColor;
            float _WindStrength;
            float _WindSpeed;
            float _WindFreq;
            float _FlagWave;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;

                float3 pos = input.positionOS.xyz;
                float2 uv = input.uv;

                // UV.x 控制固定端（x=0 固定，x=1 自由端飄動最大）
                float fixFactor = uv.x;
                fixFactor = fixFactor * fixFactor; // 非線性：靠近固定端移動極少

                float t = _Time.y * _WindSpeed;

                // 多層 sin 波模擬布料波動
                float wave1 = sin(uv.x * _WindFreq * 3.14159 + t) * _WindStrength;
                float wave2 = sin(uv.x * _WindFreq * 6.28318 + t * 1.3 + uv.y * 2.0) * _WindStrength * 0.3;
                float wave3 = sin(uv.y * _WindFreq * 2.0 + t * 0.7) * _WindStrength * 0.15;

                // Z 方向飄動（主要風向）
                pos.z += (wave1 + wave2) * fixFactor * _FlagWave;

                // Y 方向微小震盪
                pos.y += wave3 * fixFactor * _FlagWave * 0.3;

                // X 方向拉伸補償（布料展開時微縮）
                pos.x -= abs(wave1) * fixFactor * _FlagWave * 0.05;

                output.positionCS = TransformObjectToHClip(pos);
                output.positionWS = TransformObjectToWorld(pos);
                output.uv = uv;

                // 重新計算法線（基於位移的偏導數）
                float eps = 0.01;
                float3 posR = pos;
                posR.x += eps;
                float fixR = saturate((uv.x + eps) * (uv.x + eps));
                posR.z = input.positionOS.z + (sin((uv.x + eps) * _WindFreq * 3.14159 + t) * _WindStrength +
                         sin((uv.x + eps) * _WindFreq * 6.28318 + t * 1.3 + uv.y * 2.0) * _WindStrength * 0.3) * fixR * _FlagWave;

                float3 posU = pos;
                posU.y += eps;
                float fixU = fixFactor;
                posU.z = input.positionOS.z + (wave1 + sin(uv.x * _WindFreq * 6.28318 + t * 1.3 + (uv.y + eps) * 2.0) * _WindStrength * 0.3) * fixU * _FlagWave;

                float3 tangent = normalize(posR - pos);
                float3 bitangent = normalize(posU - pos);
                float3 normal = normalize(cross(bitangent, tangent));

                output.normalWS = TransformObjectToWorldNormal(normal);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float3 N = normalize(input.normalWS);

                Light mainLight = GetMainLight();
                float3 L = normalize(mainLight.direction);

                // 雙面光照
                float NdotL = dot(N, L);
                float frontLight = saturate(NdotL) * 0.6 + 0.4;
                float backLight = saturate(-NdotL) * 0.4 + 0.2;
                float lighting = max(frontLight, backLight);

                // 旗幟圖案：橫條紋
                float stripe = step(0.5, frac(input.uv.y * 5.0));
                half3 baseColor = lerp(_BaseColor.rgb, _StripeColor.rgb, stripe);

                half3 finalColor = baseColor * lighting * mainLight.color;

                return half4(finalColor, 1);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
