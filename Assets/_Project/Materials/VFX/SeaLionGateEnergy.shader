Shader "Sea Lion/VFX/Gate Energy"
{
    Properties
    {
        [HDR] _CoreColor ("Core Color", Color) = (0.25, 1.4, 2.4, 1)
        [HDR] _EdgeColor ("Edge Color", Color) = (0.05, 0.35, 1.8, 1)
        _Intensity ("Intensity", Range(0, 3)) = 1
        _Opacity ("Opacity", Range(0, 1)) = 0.7
        _FieldStrength ("Field Strength", Range(0, 1.2)) = 0.45
        _BeamStrength ("Beam Strength", Range(0, 2)) = 0.8
        _EdgeSoftness ("Edge Softness", Range(0.01, 0.8)) = 0.3
        _PulseSpeed ("Pulse Speed", Range(0, 6)) = 2.2
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Source Blend", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Destination Blend", Float) = 1
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
        Pass
        {
            Name "GateEnergy"
            Blend [_SrcBlend] [_DstBlend]
            ZWrite Off
            ZTest LEqual
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _CoreColor;
                half4 _EdgeColor;
                half _Intensity;
                half _Opacity;
                half _FieldStrength;
                half _BeamStrength;
                half _EdgeSoftness;
                half _PulseSpeed;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 centered = input.uv * 2.0 - 1.0;
                float radial = length(centered);
                half field = 1.0h - smoothstep(1.0h - _EdgeSoftness, 1.0h, radial);
                half rim = smoothstep(0.42h, 0.94h, radial) * field;
                half beam = exp2(-18.0h * abs(centered.x)) *
                    (1.0h - smoothstep(0.74h, 1.0h, abs(centered.y)));
                half shimmer = 0.88h + 0.12h * sin(_Time.y * _PulseSpeed + centered.y * 10.0h);
                half energy = saturate(field * _FieldStrength + rim * 0.38h + beam * _BeamStrength);
                half3 color = lerp(_EdgeColor.rgb, _CoreColor.rgb, saturate(beam + field * 0.2h));
                return half4(color * _Intensity, energy * _Opacity * shimmer);
            }
            ENDHLSL
        }
    }
}
