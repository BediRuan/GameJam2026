Shader "UI/ScrollingPattern"
{
    
    Properties
    {
        _MainTex    ("Texture", 2D) = "white" {}
        _Color      ("Tint", Color) = (1,1,1,1)

        // X,Y 控制滚动速度（单位：每秒多少个 pattern 宽度）
        _ScrollSpeed ("Scroll Speed (X,Y)", Vector) = (0.2, 0, 0, 0)

        // 一个小图案在屏幕上占多少像素（例如 64x64 就填 64）
        _PatternSize ("Pattern World Size (Pixels)", Float) = 64

        // ---- UI Mask / Stencil 相关 ----
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "CanUseSpriteAtlas"="False"    // 禁用图集，保证 UV 稳定
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        // ✅ 用 GUI 的深度测试，这样 3D 可以挡住它（在 Screen Space - Camera / World Space 下）
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile __ UNITY_UI_ALPHACLIP

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color  : COLOR;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos       : SV_POSITION;
                float4 color     : COLOR;
                float2 screenPos : TEXCOORD0;  // 用屏幕坐标生成 UV
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float4 _Color;
            float4 _ScrollSpeed;
            float  _PatternSize;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.color = v.color * _Color;

                // 使用屏幕坐标，而不是 v.uv（UI 的 uv 容易被图集和裁剪影响）
                float4 sp = ComputeScreenPos(o.pos);
                // 归一化到 0~1：屏幕左下 (0,0)，右上 (1,1)
                o.screenPos = sp.xy / sp.w;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 屏幕归一化坐标 → 像素坐标（以屏幕分辨率为基准）
                float2 pixelUV = i.screenPos * _ScreenParams.xy;

                // 每 _PatternSize 像素重复一次纹理
                float2 uv = pixelUV / _PatternSize;

                // 按时间滚动（_ScrollSpeed.xy 单位：pattern/秒）
                uv += _ScrollSpeed.xy * _Time.y;

                // 不手动 frac，直接交给 WrapMode = Repeat 来循环
                fixed4 col = tex2D(_MainTex, uv) * i.color;

                #ifdef UNITY_UI_ALPHACLIP
                if (col.a < 0.001)
                    discard;
                #endif

                return col;
            }
            ENDCG
        }
    }
}