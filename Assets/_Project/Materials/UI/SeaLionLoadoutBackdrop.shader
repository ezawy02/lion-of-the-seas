Shader "SeaLion/UI/LoadoutBackdrop"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _TopColor ("Top", Color) = (0.035,0.16,0.20,1)
        _BottomColor ("Bottom", Color) = (0.008,0.035,0.055,1)
        _SeaGlow ("Sea Glow", Color) = (0.02,0.48,0.50,1)
        _GoldGlow ("Gold Glow", Color) = (0.95,0.62,0.18,1)
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "CanUseSpriteAtlas"="True"
        }
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TopColor;
            fixed4 _BottomColor;
            fixed4 _SeaGlow;
            fixed4 _GoldGlow;

            v2f vert(appdata input)
            {
                v2f output;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.uv = input.uv;
                output.color = input.color * _Color;
                return output;
            }

            fixed4 frag(v2f input) : SV_Target
            {
                float2 uv = input.uv;
                float3 color = lerp(_BottomColor.rgb, _TopColor.rgb, smoothstep(0.0, 1.0, uv.y));

                float2 tealDelta = (uv - float2(0.18, 0.82)) * float2(1.0, 1.35);
                float tealGlow = exp(-dot(tealDelta, tealDelta) * 5.5);
                color += _SeaGlow.rgb * tealGlow * 0.20;

                float2 goldDelta = (uv - float2(0.88, 0.18)) * float2(1.1, 1.4);
                float goldGlow = exp(-dot(goldDelta, goldDelta) * 8.0);
                color += _GoldGlow.rgb * goldGlow * 0.055;

                float meridian = abs(frac(uv.x * 10.0 + sin(uv.y * 7.0) * 0.045) - 0.5);
                float latitude = abs(frac(uv.y * 16.0) - 0.5);
                float grid = smoothstep(0.492, 0.497, max(meridian, latitude));
                color += _SeaGlow.rgb * grid * 0.025;

                float contourA = abs(sin(uv.x * 11.0 + uv.y * 15.0 + sin(uv.y * 8.0)));
                float contourB = abs(sin(uv.x * 8.0 - uv.y * 19.0));
                float contours = smoothstep(0.965, 0.992, max(contourA, contourB));
                color += float3(0.14, 0.55, 0.56) * contours * 0.035;

                float2 vignetteUv = uv * 2.0 - 1.0;
                float vignette = saturate(1.0 - dot(vignetteUv, vignetteUv) * 0.26);
                color *= lerp(0.72, 1.0, vignette);
                return fixed4(color, 1.0) * input.color;
            }
            ENDCG
        }
    }
}
