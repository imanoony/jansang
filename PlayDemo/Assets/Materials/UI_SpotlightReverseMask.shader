Shader "UI/SpotlightReverseMask"
{
    Properties
    {
        _Color ("Overlay Color (RGB) Alpha=Darkness", Color) = (0,0,0,0.75)
        _Center("Center (UV)", Vector) = (0.5, 0.5, 0, 0)
        _Radius("Radius (UV)", Float) = 0.15
        _Softness("Softness (UV)", Float) = 0.02
        _Aspect("Aspect (W/H)", Float) = 1

        // UI 기본 텍스처(사실상 흰색이면 됨)
        [PerRendererData]_MainTex("Sprite Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

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
                float2 uv       : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _Color;
            float4 _Center;
            float _Radius;
            float _Softness;
            float _Aspect;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // 기본 오버레이 색 (알파가 어두움 정도)
                fixed4 col = _Color;

                // 원형 거리(uv space)
                float2 d = i.uv - _Center.xy;
                d.x *= _Aspect;          // ✅ 가로 비율 보정
                float dist = length(d);

                // dist <= radius => 구멍(투명)
                // softness로 가장자리 부드럽게
                float edge0 = _Radius;
                float edge1 = _Radius + max(_Softness, 1e-6);

                // 0(inside) -> 1(outside)
                float outside = smoothstep(edge0, edge1, dist);

                // inside: alpha 0, outside: alpha = _Color.a
                col.a *= outside;

                return col;
            }
            ENDCG
        }
    }
}