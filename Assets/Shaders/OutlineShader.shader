Shader "Custom/OutlineShader"
{
    Properties
    {
        _OutlineColor ("Outline Color", Color) = (1,1,0,1) // 노란색
        _OutlineWidth ("Outline Width", Range (.002, 2.0)) = .005
    }
    SubShader
    {
        Tags { "Queue" = "Overlay+1" } // 렌더 큐를 Overlay보다 앞에 설정
        Pass
        {
            Name "OUTLINE"
            Tags { "LightMode" = "UniversalForward" }
            Cull Front
            ZWrite On
            ZTest LEqual
            ColorMask RGB
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float4 color : COLOR;
            };

            float _OutlineWidth;
            float4 _OutlineColor;

            Varyings vert(Attributes v)
            {
                Varyings o;
                float3 normalWS = TransformObjectToWorldNormal(v.normalOS);
                float3 worldPos = TransformObjectToWorld(v.positionOS).xyz; // float3 반환
                float3 offsetPos = worldPos + normalWS * _OutlineWidth;
                o.positionHCS = TransformWorldToHClip(float4(offsetPos, 1.0)); // float4로 변환
                o.color = _OutlineColor;
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                return i.color;
            }
            ENDHLSL
        }
    }
}
