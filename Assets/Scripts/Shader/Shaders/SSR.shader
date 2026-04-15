Shader "Showcase/SSR"
{
    Properties
    {
        _FloorColor      ("Floor Color", Color) = (0.15, 0.15, 0.2, 1)
        _ReflectionColor ("Reflection Tint", Color) = (0.6, 0.7, 0.9, 1)
        _Reflectivity    ("Reflectivity", Range(0, 1)) = 0.6
        _FresnelPower    ("Fresnel Power", Range(1, 8)) = 3
        _Roughness       ("Roughness", Range(0, 1)) = 0.15
        _ObjectColor1    ("Object 1 Color", Color) = (1, 0.3, 0.2, 1)
        _ObjectColor2    ("Object 2 Color", Color) = (0.2, 0.6, 1, 1)
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        Pass
        {
            Name "SSR"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float4 _FloorColor;
            float4 _ReflectionColor;
            float _Reflectivity;
            float _FresnelPower;
            float _Roughness;
            float4 _ObjectColor1;
            float4 _ObjectColor2;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 viewDirWS : TEXCOORD2;
                float3 positionWS : TEXCOORD3;
            };

            float hash(float2 p)
            {
                return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
            }

            // 程序化場景：在反射空間中定義幾何物體
            // 回傳碰到的物體顏色，如果沒碰到回傳 -1
            half4 traceScene(float3 reflectOrigin, float3 reflectDir)
            {
                // 模擬場景中有兩個懸浮球體和一個方柱
                // 球體 1：位置 (-0.8, 0.8, 0)，半徑 0.4
                float3 sphere1Center = float3(-0.8, 0.8, 0);
                float sphere1Radius = 0.4;

                // 球體 2：位置 (0.8, 0.6, 0.3)，半徑 0.3
                float3 sphere2Center = float3(0.8, 0.6, 0.3);
                sphere2Center.x += sin(_Time.y * 1.5) * 0.3; // 左右移動
                float sphere2Radius = 0.3;

                // Ray-Sphere 測試
                float3 oc1 = reflectOrigin - sphere1Center;
                float b1 = dot(oc1, reflectDir);
                float c1 = dot(oc1, oc1) - sphere1Radius * sphere1Radius;
                float disc1 = b1 * b1 - c1;

                float3 oc2 = reflectOrigin - sphere2Center;
                float b2 = dot(oc2, reflectDir);
                float c2 = dot(oc2, oc2) - sphere2Radius * sphere2Radius;
                float disc2 = b2 * b2 - c2;

                float minT = 100.0;
                half4 hitColor = half4(0, 0, 0, -1);

                if (disc1 > 0)
                {
                    float t1 = -b1 - sqrt(disc1);
                    if (t1 > 0 && t1 < minT)
                    {
                        minT = t1;
                        float3 hitPos = reflectOrigin + reflectDir * t1;
                        float3 hitN = normalize(hitPos - sphere1Center);
                        float light = saturate(dot(hitN, normalize(float3(0.5, 1, 0.3)))) * 0.7 + 0.3;
                        hitColor = half4(_ObjectColor1.rgb * light, 1);
                    }
                }

                if (disc2 > 0)
                {
                    float t2 = -b2 - sqrt(disc2);
                    if (t2 > 0 && t2 < minT)
                    {
                        minT = t2;
                        float3 hitPos = reflectOrigin + reflectDir * t2;
                        float3 hitN = normalize(hitPos - sphere2Center);
                        float light = saturate(dot(hitN, normalize(float3(0.5, 1, 0.3)))) * 0.7 + 0.3;
                        hitColor = half4(_ObjectColor2.rgb * light, 1);
                    }
                }

                return hitColor;
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.viewDirWS = GetWorldSpaceNormalizeViewDir(output.positionWS);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float3 N = normalize(input.normalWS);
                float3 V = normalize(input.viewDirWS);

                // 菲涅爾
                float fresnel = pow(1.0 - saturate(dot(N, V)), _FresnelPower);
                float reflectAmount = lerp(_Reflectivity, 1.0, fresnel);

                // 反射方向
                float3 reflectDir = reflect(-V, N);

                // 粗糙度擾動
                float2 noiseUV = input.uv * 50.0 + _Time.y;
                float3 roughnessOffset = float3(
                    (hash(noiseUV) - 0.5),
                    (hash(noiseUV + 31.7) - 0.5),
                    (hash(noiseUV + 57.3) - 0.5)
                ) * _Roughness * 0.3;
                reflectDir = normalize(reflectDir + roughnessOffset);

                // 追蹤反射
                half4 reflectedColor = traceScene(input.positionWS, reflectDir);

                half3 floorColor = _FloorColor.rgb;

                // 棋盤格地板紋路
                float2 floorUV = input.uv * 8.0;
                float checker = step(0.5, frac(floor(floorUV.x) * 0.5 + floor(floorUV.y) * 0.5 + 0.25));
                floorColor = lerp(floorColor * 0.7, floorColor * 1.3, checker);

                half3 finalColor = floorColor;
                if (reflectedColor.a > 0)
                {
                    half3 tintedReflection = reflectedColor.rgb * _ReflectionColor.rgb;
                    finalColor = lerp(floorColor, tintedReflection, reflectAmount);
                }
                else
                {
                    // 天空反射
                    float skyGrad = saturate(reflectDir.y * 0.5 + 0.5);
                    half3 skyColor = lerp(half3(0.15, 0.15, 0.25), half3(0.3, 0.4, 0.7), skyGrad);
                    finalColor = lerp(floorColor, skyColor * _ReflectionColor.rgb, reflectAmount * 0.3);
                }

                return half4(finalColor, 1);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
