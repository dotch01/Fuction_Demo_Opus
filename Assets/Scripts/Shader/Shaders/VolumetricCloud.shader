Shader "Showcase/VolumetricCloud"
{
    Properties
    {
        _CloudColor    ("Cloud Color", Color) = (1, 1, 1, 1)
        _ShadowColor   ("Shadow Color", Color) = (0.4, 0.45, 0.6, 1)
        _Density       ("Density", Range(0.1, 5)) = 1.5
        _Absorption    ("Absorption", Range(0, 2)) = 0.6
        _Steps         ("Ray Steps", Range(8, 80)) = 40
        _CloudScale    ("Cloud Scale", Range(1, 10)) = 3
        _WindSpeed     ("Wind Speed", Range(0, 2)) = 0.3
        _LightIntensity("Light Intensity", Range(0.5, 5)) = 2
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" }

        Pass
        {
            Name "VolumetricCloud"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float4 _CloudColor;
            float4 _ShadowColor;
            float _Density;
            float _Absorption;
            float _Steps;
            float _CloudScale;
            float _WindSpeed;
            float _LightIntensity;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 viewDirWS : TEXCOORD2;
            };

            // 3D 雜訊
            float hash3D(float3 p)
            {
                p = frac(p * float3(0.1031, 0.1030, 0.0973));
                p += dot(p, p.yxz + 33.33);
                return frac((p.x + p.y) * p.z);
            }

            float noise3D(float3 p)
            {
                float3 i = floor(p);
                float3 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);

                float n000 = hash3D(i);
                float n100 = hash3D(i + float3(1, 0, 0));
                float n010 = hash3D(i + float3(0, 1, 0));
                float n110 = hash3D(i + float3(1, 1, 0));
                float n001 = hash3D(i + float3(0, 0, 1));
                float n101 = hash3D(i + float3(1, 0, 1));
                float n011 = hash3D(i + float3(0, 1, 1));
                float n111 = hash3D(i + float3(1, 1, 1));

                float n00 = lerp(n000, n100, f.x);
                float n10 = lerp(n010, n110, f.x);
                float n01 = lerp(n001, n101, f.x);
                float n11 = lerp(n011, n111, f.x);

                float n0 = lerp(n00, n10, f.y);
                float n1 = lerp(n01, n11, f.y);

                return lerp(n0, n1, f.z);
            }

            float fbm3D(float3 p)
            {
                float v = 0.0;
                float amp = 0.5;
                float freq = 1.0;
                for (int fi = 0; fi < 5; fi++)
                {
                    v += amp * noise3D(p * freq);
                    freq *= 2.01;
                    amp *= 0.5;
                }
                return v;
            }

            // 雲密度
            float cloudDensity(float3 pos)
            {
                float3 windOffset = float3(_Time.y * _WindSpeed, 0, _Time.y * _WindSpeed * 0.3);
                float3 samplePos = pos * _CloudScale + windOffset;

                float n = fbm3D(samplePos);

                // 垂直漸變：雲層集中在中部
                float heightFade = 1.0 - abs(pos.y - 0.5) * 2.0;
                heightFade = saturate(heightFade);
                heightFade *= heightFade;

                float density = saturate(n * 1.8 - 0.4) * heightFade * _Density;
                return density;
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.viewDirWS = GetWorldSpaceNormalizeViewDir(output.positionWS);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // 在 UV 空間做 Ray March（模擬正面觀察一個體積）
                int numSteps = (int)_Steps;
                float stepSize = 1.0 / (float)numSteps;

                // 光線方向（從前往後 z 軸深度）
                float3 rayOrigin = float3(input.uv, 0);
                float3 rayDir = float3(0, 0, 1);

                // 光源方向
                float3 lightDir = normalize(float3(0.5, 0.7, -0.5));

                float transmittance = 1.0;
                half3 accColor = half3(0, 0, 0);

                [loop]
                for (int si = 0; si < 80; si++)
                {
                    if (si >= numSteps) break;
                    if (transmittance < 0.01) break;

                    float t = ((float)si + 0.5) * stepSize;
                    float3 pos = rayOrigin + rayDir * t;

                    float density = cloudDensity(pos);

                    if (density > 0.001)
                    {
                        // 光線步進：朝光源方向採樣幾步計算自遮擋
                        float lightAtten = 0.0;
                        float lightStepSize = 0.15;
                        for (int lj = 0; lj < 4; lj++)
                        {
                            float3 lightPos = pos + lightDir * lightStepSize * ((float)lj + 1.0);
                            lightAtten += cloudDensity(lightPos);
                        }

                        float lightTransmit = exp(-lightAtten * _Absorption * lightStepSize * 4.0);

                        // 顏色：光照面亮，遮擋面暗
                        half3 sampleColor = lerp(_ShadowColor.rgb, _CloudColor.rgb * _LightIntensity, lightTransmit);

                        // Beer-Lambert 吸收
                        float sampleAbsorption = density * stepSize * _Absorption;
                        float sampleTransmittance = exp(-sampleAbsorption);

                        accColor += sampleColor * density * stepSize * transmittance;
                        transmittance *= sampleTransmittance;
                    }
                }

                float alpha = 1.0 - transmittance;
                return half4(accColor, alpha);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
