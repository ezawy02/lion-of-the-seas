Shader "Sea Lion/Environment/Natural Sand"
{
    Properties
    {
        _DryColor("Dry Sand", Color) = (0.72,0.48,0.24,1)
        _WetColor("Wet Sand", Color) = (0.36,0.22,0.12,1)
        _Variation("Natural Variation", Range(0,0.35)) = 0.13
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _DryColor;
                half4 _WetColor;
                half _Variation;
            CBUFFER_END

            struct Attributes { float4 positionOS:POSITION; half3 normalOS:NORMAL; };
            struct Varyings { float4 positionCS:SV_POSITION; float3 positionWS:TEXCOORD0; half3 normalWS:TEXCOORD1; half fog:TEXCOORD2; };

            float Hash21(float2 p)
            {
                p=frac(p*float2(123.34,456.21));
                p+=dot(p,p+45.32);
                return frac(p.x*p.y);
            }
            float Noise(float2 p)
            {
                float2 i=floor(p),f=frac(p); f=f*f*(3.0-2.0*f);
                return lerp(lerp(Hash21(i),Hash21(i+float2(1,0)),f.x),lerp(Hash21(i+float2(0,1)),Hash21(i+1),f.x),f.y);
            }
            Varyings Vert(Attributes input)
            {
                Varyings o;
                VertexPositionInputs p=GetVertexPositionInputs(input.positionOS.xyz);
                o.positionCS=p.positionCS; o.positionWS=p.positionWS;
                o.normalWS=TransformObjectToWorldNormal(input.normalOS);
                o.fog=ComputeFogFactor(p.positionCS.z);
                return o;
            }
            half4 Frag(Varyings input):SV_Target
            {
                float broad=Noise(input.positionWS.xz*0.11);
                float grain=Noise(input.positionWS.xz*0.73+3.7);
                float shore=79.0-input.positionWS.x*0.18+sin(input.positionWS.x*0.22)*2.4;
                half distanceFromShore=max(0.0h,input.positionWS.z-shore);
                half wet=saturate(1.0h-distanceFromShore*0.075h);
                half3 color=lerp(_DryColor.rgb,_WetColor.rgb,wet*0.46h);
                color*=1.0h+(broad-0.5h)*_Variation+(grain-0.5h)*_Variation*0.35h;
                half foamBand=1.0h-smoothstep(0.15h,1.15h,abs(input.positionWS.z-shore));
                color=lerp(color,half3(0.88h,0.91h,0.78h),foamBand*0.72h);
                Light sun=GetMainLight();
                half ndl=saturate(dot(normalize(input.normalWS),sun.direction));
                color*=lerp(0.82h,1.07h,ndl)*lerp(1.0h.xxx,sun.color,0.16h);
                color=MixFog(color,input.fog);
                return half4(color,1);
            }
            ENDHLSL
        }
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
    }
}
