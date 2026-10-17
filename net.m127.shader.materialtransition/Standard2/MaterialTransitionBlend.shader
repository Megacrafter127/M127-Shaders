Shader "Megacrafter127/MaterialTransition/3 Way/MaterialTransition  (Blend)"
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
        //Mat3
        _Color3("Color", Color) = (1,1,1,1)
        _MainTex3("Albedo (RGB)", 2D) = "white" {}
        _MetallicGlossMap3("Metallic (R)", 2D) = "white" {}
        _Glossiness3("Smoothness", Range(0,1)) = 0.5
        _Metallic3("Metallic", Range(0,1)) = 0.0
        [Toggle(_SMOOTHNESS_TEXTURE_ALBEDO2_CHANNEL_A)] _SmoothnessTextureChannel3("Smoothness Source", Float) = 0
        [Normal] _BumpMap3("Normal Map", 2D) = "bump" {}
        _BumpScale3("Normal Map Strength", Float) = 1
        [HDR] _EmissionMap3("Emission", 2D) = "white" {}
        [HDR] _EmissionColor3("Emission Color", Color) = (1,1,1,1)
        //Transition
        _Offset("Position Offset", Vector) = (0,0,0,0)
        _BoundingBoxMin("Low Corner of the mesh Bounding Box", Vector) = (-1,-1,-1,0)
        _BoundingBoxMax("Upper Corner of the mesh Bounding Box", Vector) = (1,1,1,0)
        _Noise("Noise", 2D) = "grey" {}
        _NoiseStrength("Noise Strength", Range(0,1)) = .1
        _SourceVector("Source", Vector) = (1,1,1,0)
        [Toggle(DIRECTION_SOURCE)] _Source_Type("Directional Source", Float) = 0
        _Shift("Shift distance", Range(0,.5)) = 0
        _Completion("Completion", Range(0,1)) = .333333333
        _Completion2("Transition", Range(0,1)) = .666666666

    }
    CustomEditor "M127.MaterialTransitionShaderGUI2"
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 300

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows noinstancing alpha:blend
        #pragma shader_feature_local DIRECTION_SOURCE
        #pragma shader_feature_local _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
        #pragma shader_feature_local _SMOOTHNESS_TEXTURE_ALBEDO2_CHANNEL_A
        #pragma shader_feature_local _SMOOTHNESS_TEXTURE_ALBEDO3_CHANNEL_A
        #pragma shader_feature_local _NORMALMAP
        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.5

        #include "./matTransStandard2.cginc"

        ENDCG
    }
    FallBack "Standard"
}
