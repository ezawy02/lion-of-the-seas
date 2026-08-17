Shader "Sea Lion/Water/Styled Mobile"
{
    Properties
    {
        [Header(Surface)] _ShallowColor ("Shallow Color", Color) = (0.055, 0.55, 0.61, 0.82)
        _DeepColor ("Deep Color", Color) = (0.015, 0.09, 0.20, 0.96)
        _FoamColor ("Foam Color", Color) = (0.87, 0.98, 0.90, 1)
        _Opacity ("Opacity", Range(0, 1)) = 0.88
        _WaveAmplitude ("Wave Amplitude", Range(0, 0.25)) = 0.045
        _WaveFrequency ("Wave Frequency", Range(0.1, 8)) = 1.8
        _WaveSpeed ("Wave Speed", Range(0, 2)) = 0.32
        _FoamScale ("Foam Scale", Range(0.2, 8)) = 2.2
        _FoamStrength ("Foam Strength", Range(0, 2)) = 0.42
        _ReducedMode ("Reduced Mode", Range(0, 1)) = 0
        [Header(Pooled Effect)] _EffectMode ("Effect Mode", Range(0, 1)) = 0
        _EffectProgress ("Effect Progress", Range(0, 1)) = 0
        _EffectIntensity ("Effect Intensity", Range(0, 2)) = 1
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" "RenderType" = "Transparent" }
        LOD 100

        Pass
        {
            Name "WaterForward"
            Tags { "LightMode" = "UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 2.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _ShallowColor;
                half4 _DeepColor;
                half4 _FoamColor;
                half _Opacity;
                half _WaveAmplitude;
                half _WaveFrequency;
                half _WaveSpeed;
                half _FoamScale;
                half _FoamStrength;
                half _ReducedMode;
                half _EffectMode;
                half _EffectProgress;
                half _EffectIntensity;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float2 uv : TEXCOORD1;
                half4 color : COLOR;
            };

            float Wave(float2 position)
            {
                float primary = sin(position.x * _WaveFrequency + _Time.y * _WaveSpeed);
                float cross = cos(position.y * (_WaveFrequency * 0.73) - _Time.y * (_WaveSpeed * 0.71));
                return (primary + cross) * 0.5;
            }

            Varyings Vert(Attributes input)
            {
                Varyings output;
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float surfaceWave = Wave(positionWS.xz) * _WaveAmplitude * (1.0 - _ReducedMode * 0.72);
                positionWS.y += surfaceWave * (1.0 - _EffectMode);
                output.positionWS = positionWS;
                output.positionHCS = TransformWorldToHClip(positionWS);
                output.uv = input.uv;
                output.color = input.color;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half waterPattern = (half)(0.5 + 0.5 * sin(input.positionWS.x * _FoamScale + _Time.y * 0.22)
                    * cos(input.positionWS.z * (_FoamScale * 0.81) - _Time.y * 0.17));
                half depthGradient = saturate(input.uv.y * 0.78 + waterPattern * 0.22);
                half3 waterColor = lerp(_DeepColor.rgb, _ShallowColor.rgb, depthGradient);

                half meshFoam = saturate(input.color.a * _FoamStrength * _EffectIntensity);
                half movingFoam = saturate(0.56 + waterPattern * 0.44);
                half foam = meshFoam * movingFoam;
                half3 finalColor = lerp(waterColor, _FoamColor.rgb, foam);
                half alpha = saturate(lerp(_Opacity, _FoamColor.a, foam) * input.color.r);

                // Effects fade by scale in the component, while this term keeps the tail readable.
                alpha *= lerp(1.0, saturate(1.0 - _EffectProgress * 0.92), _EffectMode);
                return half4(finalColor, alpha);
            }
            ENDHLSL
        }
    }
}
