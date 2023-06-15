Shader "Megacrafter127/MaterialTransition"
{
    Properties
    {
        //Mat1
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("Albedo (RGB)", 2D) = "white" {}
        _MetallicGlossMap("Metallic (R)", 2D) = "white" {}
        _Glossiness("Smoothness", Range(0,1)) = 0.5
        _Metallic("Metallic", Range(0,1)) = 0.0
        [Toggle(_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A)] _SmoothnessTextureChannel("Smoothness Source", Float) = 0
        [Normal] _BumpMap("Normal Map", 2D) = "bump" {}
        _BumpScale("Normal Map Strength", Float) = 1
        [HDR] _EmissionMap("Emission", 2D) = "white" {}
        [HDR] _EmissionColor("Emission Color", Color) = (1,1,1,1)
        //Mat2
        _Color2("Color", Color) = (1,1,1,1)
        _MainTex2("Albedo (RGB)", 2D) = "white" {}
        _MetallicGlossMap2("Metallic (R)", 2D) = "white" {}
        _Glossiness2("Smoothness", Range(0,1)) = 0.5
        _Metallic2("Metallic", Range(0,1)) = 0.0
        [Toggle(_SMOOTHNESS_TEXTURE_ALBEDO2_CHANNEL_A)] _SmoothnessTextureChannel2("Smoothness Source", Float) = 0
        [Normal] _BumpMap2("Normal Map", 2D) = "bump" {}
        _BumpScale2("Normal Map Strength", Float) = 1
        [HDR] _EmissionMap2("Emission", 2D) = "white" {}
        [HDR] _EmissionColor2("Emission Color", Color) = (1,1,1,1)
        //Transition
        _Offset("Position Offset", Vector) = (0,0,0,0)
        _BoundingBoxMin("Low Corner of the mesh Bounding Box", Vector) = (-1,-1,-1,0)
        _BoundingBoxMax("Upper Corner of the mesh Bounding Box", Vector) = (1,1,1,0)
        _Noise("Noise", 2D) = "grey" {}
        _NoiseStrength("Noise Strength", Range(0,1)) = .1
        _SourceVector("Source", Vector) = (1,1,1,0)
        [Toggle(DIRECTION_SOURCE)] _Source_Type("Directional Source", Float) = 0
        _Shift("Shift distance", Range(0,.5)) = 0
        _Completion("Completion", Range(0,1)) = .5
    }
    CustomEditor "M127.MaterialTransitionShaderGUI"
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 300

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows noinstancing
        #pragma shader_feature_local DIRECTION_SOURCE
        #pragma shader_feature_local _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
        #pragma shader_feature_local _SMOOTHNESS_TEXTURE_ALBEDO2_CHANNEL_A
        #pragma shader_feature_local _NORMALMAP
        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.5

        sampler2D _MainTex,
            _MetallicGlossMap,
#ifdef _NORMALMAP
            _BumpMap,
#endif
            _EmissionMap;

        sampler2D _MainTex2,
            _MetallicGlossMap2,
#ifdef _NORMALMAP
            _BumpMap2,
#endif
            _EmissionMap2;

        sampler2D _Normalized_Position,
            _Noise;

        struct Input
        {
            float2 uv_MainTex,
                uv_MetallicGlossMap,
#ifdef _NORMALMAP
                uv_BumpMap,
#endif
                uv_EmissionMap;

            float2 uv_MainTex2,
                uv_MetallicGlossMap2,
#ifdef _NORMALMAP
                uv_BumpMap2,
#endif
                uv_EmissionMap2;

            float4 vcol : COLOR;
            float2 uv_Noise;
        };

        float _Glossiness;
        float _Metallic;
        float4 _Color;
        float _BumpScale;
        float3 _EmissionColor;
        float _Glossiness2;
        float _Metallic2;
        float4 _Color2;
        float _BumpScale2;
        float3 _EmissionColor2;

        float3 _Offset;
        float3 _BoundingBoxMin, _BoundingBoxMax;
        float3 _SourceVector;
        float _NoiseStrength;
        float _Shift;
        float _Completion;

#include "../matTrans.cginc"

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float3 pos = IN.vcol.rgb + _Offset;
#ifdef DIRECTION_SOURCE
            float grad = gradientDir(pos, _SourceVector, _BoundingBoxMin, _BoundingBoxMax);
#else
            float grad = gradientPoint(pos, _SourceVector, _BoundingBoxMin, _BoundingBoxMax);
#endif
            grad = lerp(grad, tex2D(_Noise, IN.uv_Noise).r, _NoiseStrength);

            grad = distLerp(grad, _Completion, _Shift);
            // Albedo comes from a texture tinted by color
            float4 c1 = tex2D(_MainTex, IN.uv_MainTex), c2 = tex2D(_MainTex2, IN.uv_MainTex2);
            float4 c = lerp(c1 * _Color, c2 * _Color2, grad);
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            float4 m = tex2D(_MetallicGlossMap, IN.uv_MetallicGlossMap);
            float4 m2 = tex2D(_MetallicGlossMap2, IN.uv_MetallicGlossMap2);
            o.Metallic = lerp(m.r * _Metallic, m2.r * _Metallic2, grad);
#ifdef _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            m.a = c1.a;
#endif
#ifdef _SMOOTHNESS_TEXTURE_ALBEDO2_CHANNEL_A
            m2.a = c2.a;
#endif
            o.Smoothness = lerp(m.a * _Glossiness, m2.a * _Glossiness2, grad);
#ifdef _NORMALMAP
            float normalS = lerp(_BumpScale, _BumpScale2, grad);
            o.Normal = lerp(float3(0, 0, 1), UnpackNormal(lerp(tex2D(_BumpMap, IN.uv_BumpMap), tex2D(_BumpMap2, IN.uv_BumpMap2), grad)), normalS);
#endif
            o.Emission = lerp(tex2D(_EmissionMap, IN.uv_EmissionMap).rgb * _EmissionColor, tex2D(_EmissionMap2, IN.uv_EmissionMap2).rgb * _EmissionColor2, grad);
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Standard"
}
