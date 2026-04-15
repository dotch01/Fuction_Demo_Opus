Shader "MathDemo/ZoneHighlight"
{
    Properties
    {
        _Color     ("Color", Color) = (0.3, 0.5, 1, 0.15)
        _EdgeColor ("Edge Color", Color) = (0.4, 0.7, 1, 0.6)
        _EdgeWidth ("Edge Width", Range(0, 0.1)) = 0.03
        _Pulse     ("Pulse Speed", Float) = 1.5
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" }

        Pass
        {
            Name "ZoneHighlight"
            Tags { "LightMode" = "UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float4 _Color;
            float4 _EdgeColor;
            float _EdgeWidth;
            float _Pulse;

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

            Varyings vert(Attributes i)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(i.positionOS.xyz);
                o.uv = i.uv;
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float2 uv = i.uv;
                float edgeX = min(uv.x, 1.0 - uv.x);
                float edgeY = min(uv.y, 1.0 - uv.y);
                float edge = min(edgeX, edgeY);

                float pulse = 0.8 + 0.2 * sin(_Time.y * _Pulse);
                float edgeMask = smoothstep(0.0, _EdgeWidth, edge);

                half4 color = lerp(_EdgeColor * pulse, _Color, edgeMask);
                return color;
            }
            ENDHLSL
        }
    }
    FallBack Off
}
