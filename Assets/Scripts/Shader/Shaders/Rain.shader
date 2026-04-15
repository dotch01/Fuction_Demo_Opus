Shader "Showcase/Rain"
{
    Properties
    {
        _RainColor  ("Rain Color", Color) = (0.7, 0.8, 0.9, 0.4)
        _Density    ("Density", Range(10, 200)) = 80
        _Speed      ("Speed", Range(1, 20)) = 8
        _Length      ("Drop Length", Range(0.01, 0.2)) = 0.08
        _Thickness   ("Drop Thickness", Range(0.001, 0.02)) = 0.005
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" }

        Pass
        {
            Name "Rain"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha One
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float4 _RainColor;
            float _Density;
            float _Speed;
            float _Length;
            float _Thickness;

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

            float hash(float2 p)
            {
                return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;
                float intensity = 0.0;

                // 多層雨滴，每層密度和速度略有不同
                for (int layer = 0; layer < 3; layer++)
                {
                    float layerScale = _Density * (0.6 + float(layer) * 0.3);
                    float layerSpeed = _Speed * (0.8 + float(layer) * 0.2);

                    float2 cellUV = uv * float2(layerScale, layerScale * 0.3);
                    cellUV.y += _Time.y * layerSpeed;

                    float2 cellID = floor(cellUV);
                    float2 cellFrac = frac(cellUV);

                    // 每個 cell 的隨機偏移
                    float randX = hash(cellID) * 0.8 + 0.1;
                    float randY = hash(cellID + 31.7);

                    // 雨滴形狀：細長的線段
                    float distX = abs(cellFrac.x - randX);
                    float distY = abs(cellFrac.y - 0.5);

                    float drop = step(distX, _Thickness) * step(distY, _Length);
                    float fade = 1.0 - distX / _Thickness;
                    fade *= 1.0 - distY / _Length;

                    intensity += drop * fade * (0.5 + float(layer) * 0.25);
                }

                intensity = saturate(intensity);
                return half4(_RainColor.rgb * intensity, _RainColor.a * intensity);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
