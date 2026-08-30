Shader "Sea Lion/Water/Styled Mobile"
{
    Properties
    {
        [Header(Surface)] _ShallowColor ("Shallow Color", Color) = (0.055, 0.55, 0.61, 0.82)
        _DeepColor ("Deep Color", Color) = (0.015, 0.09, 0.20, 0.96)
        _HorizonColor ("Horizon Reflection", Color) = (0.18, 0.42, 0.48, 1)
        _ForegroundColor ("Foreground Depth Color", Color) = (0.04, 0.33, 0.35, 1)
        _ForegroundStrength ("Foreground Depth Strength", Range(0, 1)) = 0
        _ShoreStrength ("Far Shallow Strength", Range(0, 1)) = 0.34
        _FoamColor ("Foam Color", Color) = (0.87, 0.98, 0.90, 1)
        _Opacity ("Opacity", Range(0, 1)) = 0.88
        _WaveAmplitude ("Wave Amplitude", Range(0, 0.25)) = 0.045
        _WaveFrequency ("Wave Frequency", Range(0.1, 8)) = 1.8
        _WaveSpeed ("Wave Speed", Range(0, 2)) = 0.32
        _NormalStrength ("Normal Strength", Range(0, 4)) = 1.25
        _FresnelStrength ("Fresnel Strength", Range(0, 2)) = 0.72
        _SpecularStrength ("Sun Glitter", Range(0, 3)) = 0.8
        _FoamScale ("Foam Scale", Range(0.2, 8)) = 2.2
        _FoamStrength ("Foam Strength", Range(0, 2)) = 0.42
        _ReducedMode ("Reduced Mode", Range(0, 1)) = 0
        [Header(Pooled Effect)] _EffectMode ("Effect Mode", Range(0, 1)) = 0
        _EffectProgress ("Effect Progress", Range(0, 1)) = 0
        _EffectIntensity ("Effect Intensity", Range(0, 2)) = 1
        _EffectAlphaBoost ("Effect Alpha Boost", Range(1, 3)) = 1
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
            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _ShallowColor;
                half4 _DeepColor;
                half4 _HorizonColor;
                half4 _ForegroundColor;
                half4 _FoamColor;
                half _Opacity;
                half _WaveAmplitude;
                half _WaveFrequency;
                half _WaveSpeed;
                half _NormalStrength;
                half _FresnelStrength;
                half _ForegroundStrength;
                half _ShoreStrength;
                half _SpecularStrength;
                half _FoamScale;
                half _FoamStrength;
                half _ReducedMode;
                half _EffectMode;
                half _EffectProgress;
                half _EffectIntensity;
                half _EffectAlphaBoost;
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

            float Hash21(float2 value)
            {
                value = frac(value * float2(123.34, 456.21));
                value += dot(value, value + 45.32);
                return frac(value.x * value.y);
            }

            float ValueNoise(float2 value)
            {
                float2 cell = floor(value);
                float2 local = frac(value);
                local = local * local * (3.0 - 2.0 * local);
                float a = Hash21(cell);
                float b = Hash21(cell + float2(1, 0));
                float c = Hash21(cell + float2(0, 1));
                float d = Hash21(cell + float2(1, 1));
                return lerp(lerp(a, b, local.x), lerp(c, d, local.x), local.y);
            }

            float OceanNoise(float2 position)
            {
                float time = _Time.y * _WaveSpeed;
                float broad = ValueNoise(position * 0.16 + float2(time * 0.07, -time * 0.05));
                float detail = ValueNoise(position * 0.43 + float2(-time * 0.13, time * 0.09));
                float micro = ValueNoise(position * 0.91 + float2(time * 0.18, time * 0.12));
                return broad * 0.56 + detail * 0.31 + micro * 0.13;
            }

            float BrokenCrest(float2 position, float2 direction, float frequency, float phase)
            {
                float time = _Time.y * _WaveSpeed;
                float warp = ValueNoise(position * 0.19 + direction.yx * time * 0.08 + phase) * 5.7;
                float crestSignal = sin(dot(position, direction) * frequency + warp + time * phase);
                float breakup = ValueNoise(position * 0.58 - direction * time * 0.11 + phase * 2.1);
                return pow(saturate(crestSignal * 0.5 + 0.5), 26.0) * smoothstep(0.40, 0.76, breakup);
            }

            float WaveShape(float2 position)
            {
                float time = _Time.y * _WaveSpeed;
                float frequency = _WaveFrequency;
                float2 warped = position + float2(
                    sin(position.y * 0.13 + time * 0.09),
                    cos(position.x * 0.11 - time * 0.07)) * 1.35;
                float a = sin(dot(warped, float2(0.82, 0.57)) * frequency + time);
                float b = sin(dot(warped, float2(-0.46, 0.89)) * frequency * 0.67 - time * 0.73 + 1.7);
                float c = sin(dot(warped, float2(0.96, -0.28)) * frequency * 1.43 + time * 1.21 + 3.1);
                float d = sin(dot(warped, float2(-0.91, -0.41)) * frequency * 2.17 - time * 1.46 + 0.6);
                float e = sin(dot(warped, float2(0.31, 0.95)) * frequency * 3.11 + time * 1.83 + 4.2);
                return a * 0.30 + b * 0.24 + c * 0.19 + d * 0.16 + e * 0.11;
            }

            float SurfaceHeight(float2 position)
            {
                return WaveShape(position) * _WaveAmplitude;
            }

            float3 SurfaceNormal(float2 position)
            {
                float time = _Time.y * _WaveSpeed;
                float frequency = _WaveFrequency;
                float2 d0 = float2(0.82, 0.57) * frequency;
                float2 d1 = float2(-0.46, 0.89) * frequency * 0.67;
                float2 d2 = float2(0.96, -0.28) * frequency * 1.43;
                float2 d3 = float2(-0.91, -0.41) * frequency * 2.17;
                float2 d4 = float2(0.31, 0.95) * frequency * 3.11;
                float s0 = cos(dot(position, float2(0.82, 0.57)) * frequency + time);
                float s1 = cos(dot(position, float2(-0.46, 0.89)) * frequency * 0.67 - time * 0.73 + 1.7);
                float s2 = cos(dot(position, float2(0.96, -0.28)) * frequency * 1.43 + time * 1.21 + 3.1);
                float s3 = cos(dot(position, float2(-0.91, -0.41)) * frequency * 2.17 - time * 1.46 + 0.6);
                float s4 = cos(dot(position, float2(0.31, 0.95)) * frequency * 3.11 + time * 1.83 + 4.2);
                float2 slope = (d0 * s0 * 0.30 + d1 * s1 * 0.24 + d2 * s2 * 0.19
                    + d3 * s3 * 0.16 + d4 * s4 * 0.11)
                    * _WaveAmplitude * _NormalStrength * (1.0 - _ReducedMode * 0.55);
                return normalize(float3(-slope.x, 1.0, -slope.y));
            }

            Varyings Vert(Attributes input)
            {
                Varyings output;
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float surfaceWave = SurfaceHeight(positionWS.xz) * (1.0 - _ReducedMode * 0.72);
                positionWS.y += surfaceWave * (1.0 - _EffectMode);
                output.positionWS = positionWS;
                output.positionHCS = TransformWorldToHClip(positionWS);
                output.uv = input.uv;
                output.color = input.color;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 position = input.positionWS.xz;
                half waveShape = (half)WaveShape(position);
                half oceanNoise = (half)OceanNoise(position);
                half micro = (half)(sin(dot(position, float2(1.73, -1.19)) * _FoamScale
                    + _Time.y * _WaveSpeed * 1.7) * 0.5 + 0.5);
                float3 normalWS = SurfaceNormal(position);
                float3 viewDirection = SafeNormalize(GetCameraPositionWS() - input.positionWS);
                half fresnel = pow(1.0h - saturate(dot(normalWS, viewDirection)), 4.0h);
                half colorVariation = saturate(0.42h + waveShape * 0.045h
                    + micro * 0.018h + (oceanNoise - 0.5h) * 0.30h);
                half shoreShallow = smoothstep(56.0h, 94.0h, input.positionWS.z);
                half3 waterColor = lerp(_DeepColor.rgb, _ShallowColor.rgb, colorVariation);
                waterColor = lerp(waterColor, _ShallowColor.rgb, shoreShallow * _ShoreStrength);
                waterColor = lerp(waterColor, _HorizonColor.rgb, saturate(fresnel * _FresnelStrength));
                waterColor += _HorizonColor.rgb * smoothstep(0.58h, 0.88h, oceanNoise) * 0.085h;
                waterColor += _ShallowColor.rgb * (oceanNoise - 0.5h) * 0.12h;

                // Noise-warped, broken crest fragments avoid the regular grid look of
                // intersecting sine bands while staying texture-free and mobile friendly.
                half ridgeA = (half)BrokenCrest(position, float2(0.82, 0.57), 0.72, 0.63);
                half ridgeB = (half)BrokenCrest(position, float2(-0.46, 0.89), 0.54, -0.47);
                half wavelet = saturate(ridgeA * 0.72h + ridgeB * 0.46h);
                waterColor = lerp(waterColor, _FoamColor.rgb,
                    wavelet * 0.12h * (1.0h - _ReducedMode * 0.75h));

                // A restrained reflection streak gives the surface depth without a
                // reflection camera or screen-space texture fetch.
                half reflectionMask = pow(saturate(1.0h - abs(viewDirection.x) * 1.35h), 5.0h)
                    * smoothstep(0.32h, 0.82h, oceanNoise) * fresnel;
                waterColor += _HorizonColor.rgb * reflectionMask * 0.16h;

                Light mainLight = GetMainLight();
                float3 halfDirection = SafeNormalize(mainLight.direction + viewDirection);
                half sunGlitter = pow(saturate(dot(normalWS, halfDirection)), 72.0h)
                    * _SpecularStrength * mainLight.distanceAttenuation;
                half broadReflection = pow(saturate(dot(normalWS, halfDirection)), 14.0h) * 0.026h;
                half scatteredGlint = pow(saturate(oceanNoise * 0.76h + micro * 0.34h), 18.0h)
                    * saturate(dot(normalWS, halfDirection)) * 0.055h
                    * (1.0h - _ReducedMode * 0.8h);
                waterColor += mainLight.color * (sunGlitter + broadReflection + scatteredGlint);
                half foregroundMask = 1.0h - smoothstep(12.0h, 42.0h, input.positionWS.z);
                waterColor = lerp(waterColor, _ForegroundColor.rgb,
                    foregroundMask * _ForegroundStrength);

                half meshFoam = saturate(input.color.a * _FoamStrength * _EffectIntensity * _EffectMode);
                half crest = smoothstep(0.68h, 0.94h, waveShape + (micro - 0.5h) * 0.16h);
                half movingFoam = saturate(crest * lerp(0.45h, 0.86h, oceanNoise) + micro * 0.10h);
                half shorelineBand = smoothstep(91.0h, 98.0h, input.positionWS.z)
                    * (1.0h - smoothstep(104.0h, 111.0h, input.positionWS.z));
                half shorelineBreakup = smoothstep(0.46h, 0.72h,
                    (half)ValueNoise(position * 0.42 + float2(_Time.y * 0.03, 0.0)));
                half foam = saturate(crest * _FoamStrength * 0.08h + wavelet * 0.06h
                    + shorelineBand * shorelineBreakup * _FoamStrength * 0.52h);
                half effectBreakup = lerp(0.58h, 1.0h,
                    abs(sin(input.uv.x * 37.0h + input.uv.y * 13.0h + oceanNoise * 5.0h)));
                foam = lerp(foam, saturate(meshFoam * effectBreakup), _EffectMode);
                half3 finalColor = lerp(waterColor, _FoamColor.rgb, foam);
                half alpha = saturate(lerp(_Opacity, _FoamColor.a, foam));
                // Surface vertices use alpha for shoreline foam, while pooled effects
                // use it as a deterministic tail fade. Do not force wake quads opaque.
                half wakeBreakup = lerp(0.68h, 1.0h,
                    abs(sin(input.uv.x * 31.0h + input.uv.y * 8.0h + oceanNoise * 3.0h)));
                alpha *= lerp(1.0h, input.color.a * wakeBreakup, _EffectMode);
                half wakeGrain = (half)ValueNoise(position * 1.18
                    + input.uv * float2(5.7, 2.3) + float2(_Time.y * 0.08, 0.0));
                half wakeFragment = smoothstep(0.38h, 0.72h,
                    wakeGrain + abs(sin(input.uv.x * 19.0h + input.uv.y * 11.0h)) * 0.24h);
                alpha *= lerp(1.0h, wakeFragment, _EffectMode);

                // Effects fade by scale in the component, while this term keeps the tail readable.
                alpha *= lerp(1.0, saturate(1.0 - _EffectProgress * 0.92), _EffectMode);
                alpha = saturate(alpha * lerp(1.0h, _EffectAlphaBoost, _EffectMode));
                return half4(finalColor, alpha);
            }
            ENDHLSL
        }
    }
}
