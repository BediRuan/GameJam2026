Shader "Custom/ScrollingPattern"
{
    Properties
    {
        _BaseMap ("Texture", 2D) = "white" {}
        _Speed  ("Scroll Speed (xy)", Vector) = (0.1, 0.0, 0, 0)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 100

        Pass
        {
            Name "Forward"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _Speed;      // xy 用来控制速度
            CBUFFER_END

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);

                // 基础 UV 变换（Tiling/Offset）
                float2 uv = TRANSFORM_TEX(IN.uv, _BaseMap);

                // 按时间滚动：_Time.y 大概是经过的秒数
                float2 offset = _Speed.xy * _Time.y;
                uv += offset;

                // frac 保持在 0~1 之间，形成循环
                OUT.uv = frac(uv);
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                half4 col = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                return col;
            }
            ENDHLSL
        }
    }
}

