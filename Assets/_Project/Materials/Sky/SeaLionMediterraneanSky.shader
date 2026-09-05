Shader "Sea Lion/Sky/Mediterranean Procedural"
{
    Properties
    {
        _ZenithColor ("Zenith", Color) = (0.16,0.55,0.88,1)
        _HorizonColor ("Horizon", Color) = (0.55,0.82,0.93,1)
        _CloudColor ("Cloud", Color) = (0.95,0.97,0.94,1)
        _CloudStrength ("Cloud Strength", Range(0,1)) = 0.72
        _CloudScale ("Cloud Scale", Range(0.5,3)) = 1
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            half4 _ZenithColor;
            half4 _HorizonColor;
            half4 _CloudColor;
            half _CloudStrength;
            half _CloudScale;
            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 positionHCS : SV_POSITION; float3 direction : TEXCOORD0; };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.direction = TransformObjectToWorldDir(input.positionOS.xyz);
                return output;
            }
            float Hash(float2 p) { return frac(sin(dot(p, float2(127.1,311.7))) * 43758.5453); }
            float Noise(float2 p)
            {
                float2 i=floor(p), f=frac(p); f=f*f*(3.0-2.0*f);
                return lerp(lerp(Hash(i),Hash(i+float2(1,0)),f.x),lerp(Hash(i+float2(0,1)),Hash(i+1),f.x),f.y);
            }
            half4 Frag(Varyings input) : SV_Target
            {
                float3 d=normalize(input.direction);
                float height=saturate(d.y*1.45+0.12);
                half3 sky=lerp(_HorizonColor.rgb,_ZenithColor.rgb,pow(height,0.72));
                float denom=max(0.12,d.y+0.34);
                float2 p=d.xz/denom*1.35*_CloudScale;
                float n=Noise(p*1.1)*0.58+Noise(p*2.35+4.7)*0.29+Noise(p*5.2-2.1)*0.13;
                float clouds=smoothstep(0.47,0.68,n)*saturate(1.0-height*0.72)*_CloudStrength;
                sky=lerp(sky,_CloudColor.rgb,clouds);
                return half4(sky,1);
            }
            ENDHLSL
        }
    }
}
