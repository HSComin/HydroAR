Shader "HydroAR/UIRoundedRect"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Fill Color", Color) = (1,1,1,1)

        _Size ("Rect Size (px)", Vector) = (100,100,0,0)
        _Radius ("Corner Radius (px)", Float) = 16

        _BorderWidth ("Border Width (px)", Float) = 0
        _BorderColor ("Border Color", Color) = (0,0,0,0)

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                float2 localPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float4 _Size;
            float _Radius;
            float _BorderWidth;
            fixed4 _BorderColor;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(v.vertex);
                OUT.texcoord = v.texcoord;
                OUT.color = v.color * _Color;
                // posição local em pixels, centralizada em (0,0)
                OUT.localPos = (v.texcoord - 0.5) * _Size.xy;
                return OUT;
            }

            // Distância assinada de um retângulo arredondado (técnica SDF padrão)
            float sdRoundRect(float2 p, float2 halfSize, float r)
            {
                float2 q = abs(p) - halfSize + r;
                return length(max(q, 0.0)) + min(max(q.x, q.y), 0.0) - r;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 halfSize = _Size.xy * 0.5;
                float r = min(_Radius, min(halfSize.x, halfSize.y));

                float dist = sdRoundRect(IN.localPos, halfSize, r);

                // Anti-aliasing suave na borda externa
                float aa = fwidth(dist) * 1.5;
                float outerAlpha = 1.0 - smoothstep(-aa, aa, dist);

                // Amostra a textura (imagem do mapa) e multiplica pela cor/tint
                fixed4 texColor = tex2D(_MainTex, IN.texcoord);
                fixed4 col = texColor * IN.color;

                // Aplica borda, se configurada
                if (_BorderWidth > 0.001)
                {
                    float innerDist = dist + _BorderWidth;
                    float innerAlpha = 1.0 - smoothstep(-aa, aa, innerDist);
                    col.rgb = lerp(_BorderColor.rgb, col.rgb, innerAlpha);
                }

                col.a *= outerAlpha;
                return col;
            }
            ENDCG
        }
    }
}
