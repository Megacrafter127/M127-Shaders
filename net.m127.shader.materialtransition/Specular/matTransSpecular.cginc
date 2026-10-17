sampler2D _MainTex,
          _SpecGlossMap,
          #ifdef _NORMALMAP
          _BumpMap,
          #endif
          _EmissionMap;

sampler2D _MainTex2,
          _SpecGlossMap2,
          #ifdef _NORMALMAP
          _BumpMap2,
          #endif
          _EmissionMap2;
sampler2D _Noise;

struct Input
{
    float2 uv_MainTex,
           uv_SpecGlossMap,
           #ifdef _NORMALMAP
           uv_BumpMap,
           #endif
           uv_EmissionMap;

    float2 uv_MainTex2,
           uv_SpecGlossMap2,
           #ifdef _NORMALMAP
           uv_BumpMap2,
           #endif
           uv_EmissionMap2;
    float4 vcol : COLOR;
    float2 uv_Noise;
};

float _Glossiness;
float4 _Color;
float _BumpScale;
float3 _EmissionColor;
float _Glossiness2;
float4 _SpecColor2;
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

void surf(Input IN, inout SurfaceOutputStandardSpecular o)
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
    float4 m = tex2D(_SpecGlossMap, IN.uv_SpecGlossMap);
    float4 m2 = tex2D(_SpecGlossMap2, IN.uv_SpecGlossMap2);
    o.Specular = lerp(m.rgb * _SpecColor.rgb, m2.rgb * _SpecColor2.rgb, grad);
    #ifdef _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
    m.a = c1.a;
    #endif
    #ifdef _SMOOTHNESS_TEXTURE_ALBEDO2_CHANNEL_A
    m2.a = c2.a;
    #endif
    o.Smoothness = lerp(m.a * _Glossiness, m2.a * _Glossiness2, grad);
    #ifdef _NORMALMAP
    float normalS = lerp(_BumpScale, _BumpScale2, grad);
    o.Normal = lerp(float3(0, 0, 1),
                    UnpackNormal(lerp(tex2D(_BumpMap, IN.uv_BumpMap), tex2D(_BumpMap2, IN.uv_BumpMap2), grad)),
                    normalS);
    #endif
    o.Emission = lerp(tex2D(_EmissionMap, IN.uv_EmissionMap).rgb * _EmissionColor,
                      tex2D(_EmissionMap2, IN.uv_EmissionMap2).rgb * _EmissionColor2, grad);
    o.Alpha = c.a;
}
