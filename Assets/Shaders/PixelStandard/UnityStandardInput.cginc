// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

#ifndef UNITY_STANDARD_INPUT_INCLUDED
#define UNITY_STANDARD_INPUT_INCLUDED

#include "UnityCG.cginc"
#include "UnityStandardConfig.cginc"
#include "UnityPBSLighting.cginc" // TBD: remove
#include "UnityStandardUtils.cginc"

//---------------------------------------
// Directional lightmaps & Parallax require tangent space too
#if (_NORMALMAP || DIRLIGHTMAP_COMBINED || _PARALLAXMAP)
    #define _TANGENT_TO_WORLD 1
#endif

#if (_DETAIL_MULX2 || _DETAIL_MUL || _DETAIL_ADD || _DETAIL_LERP)
    #define _DETAIL 1
#endif

//---------------------------------------
half4       _Color;
half        _Cutoff;

#define DECLARE_TEX2D_LOCAL(tex) Texture2D tex; SamplerState sampler##tex; float4 tex##_TexelSize;

DECLARE_TEX2D_LOCAL(_MainTex);
float4      _MainTex_ST;

DECLARE_TEX2D_LOCAL(_DetailAlbedoMap);
float4      _DetailAlbedoMap_ST;

DECLARE_TEX2D_LOCAL(_BumpMap);
half        _BumpScale;

DECLARE_TEX2D_LOCAL(_DetailMask);
DECLARE_TEX2D_LOCAL(_DetailNormalMap);
half        _DetailNormalMapScale;

DECLARE_TEX2D_LOCAL(_SpecGlossMap);
DECLARE_TEX2D_LOCAL(_MetallicGlossMap);
half        _Metallic;
float       _Glossiness;
float       _GlossMapScale;

DECLARE_TEX2D_LOCAL(_OcclusionMap);
half        _OcclusionStrength;

DECLARE_TEX2D_LOCAL(_ParallaxMap);
half        _Parallax;
half        _UVSec;

half4       _EmissionColor;
DECLARE_TEX2D_LOCAL(_EmissionMap);

//-------------------------------------------------------------------------------------
// Input functions

float2 sharpCoords(float4 texelSize , float2 p)
{
    p = p*texelSize.zw;
    float2 c = max(0.0, abs(fwidth(p)));
    p = p + abs(c);
    p = floor(p) + saturate(frac(p) / c);
    p = (p - 0.5)*texelSize.xy;
    return p;
}

float4 sampleSharp(Texture2D target, SamplerState samplertarget, float2 texcoord, float4 texelSize)
{
    #if 1 // Not required, included for comparison/testing
    float2 res = 0;
    #ifndef SHADER_TARGET_SURFACE_ANALYSIS
    target.GetDimensions(res.x, res.y);
    #endif
    texelSize = float4(1.0/res.xy, res.xy);
    #endif

    float2 dUV = sharpCoords(texelSize, texcoord);

    return target.Sample(samplertarget, dUV);

    // This relies on the macro semantics, which seems to go against the point of using it
    //return UNITY_SAMPLE_TEX2D_SAMPLER(target, target, dUV);
}

#define SAMPLE_TEX2D_SHARP(tex, coord) sampleSharp(tex,sampler##tex,coord,tex##_TexelSize)

struct VertexInput
{
    float4 vertex   : POSITION;
    half3 normal    : NORMAL;
    float2 uv0      : TEXCOORD0;
    float2 uv1      : TEXCOORD1;
#if defined(DYNAMICLIGHTMAP_ON) || defined(UNITY_PASS_META)
    float2 uv2      : TEXCOORD2;
#endif
#ifdef _TANGENT_TO_WORLD
    half4 tangent   : TANGENT;
#endif
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

float4 TexCoords(VertexInput v)
{
    float4 texcoord;
    texcoord.xy = TRANSFORM_TEX(v.uv0, _MainTex); // Always source from uv0
    texcoord.zw = TRANSFORM_TEX(((_UVSec == 0) ? v.uv0 : v.uv1), _DetailAlbedoMap);
    return texcoord;
}

half DetailMask(float2 uv)
{
    return SAMPLE_TEX2D_SHARP (_DetailMask, uv).a;
}

half3 Albedo(float4 texcoords)
{
    half3 albedo = _Color.rgb * SAMPLE_TEX2D_SHARP (_MainTex, texcoords.xy).rgb;
#if _DETAIL
    #if (SHADER_TARGET < 30)
        // SM20: instruction count limitation
        // SM20: no detail mask
        half mask = 1;
    #else
        half mask = DetailMask(texcoords.xy);
    #endif
    half3 detailAlbedo = SAMPLE_TEX2D_SHARP (_DetailAlbedoMap, texcoords.zw).rgb;
    #if _DETAIL_MULX2
        albedo *= LerpWhiteTo (detailAlbedo * unity_ColorSpaceDouble.rgb, mask);
    #elif _DETAIL_MUL
        albedo *= LerpWhiteTo (detailAlbedo, mask);
    #elif _DETAIL_ADD
        albedo += detailAlbedo * mask;
    #elif _DETAIL_LERP
        albedo = lerp (albedo, detailAlbedo, mask);
    #endif
#endif
    return albedo;
}

half Alpha(float2 uv)
{
#if defined(_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A)
    return _Color.a;
#else
    return SAMPLE_TEX2D_SHARP (_MainTex, uv).a * _Color.a;
#endif
}

half Occlusion(float2 uv)
{
#if (SHADER_TARGET < 30)
    // SM20: instruction count limitation
    // SM20: simpler occlusion
    return SAMPLE_TEX2D_SHARP(_OcclusionMap, uv).g;
#else
    half occ = SAMPLE_TEX2D_SHARP(_OcclusionMap, uv).g;
    return LerpOneTo (occ, _OcclusionStrength);
#endif
}

half4 SpecularGloss(float2 uv)
{
    half4 sg;
#ifdef _SPECGLOSSMAP
    #if defined(_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A)
        sg.rgb = SAMPLE_TEX2D_SHARP(_SpecGlossMap, uv).rgb;
        sg.a = SAMPLE_TEX2D_SHARP(_MainTex, uv).a;
    #else
        sg = SAMPLE_TEX2D_SHARP(_SpecGlossMap, uv);
    #endif
    sg.a *= _GlossMapScale;
#else
    sg.rgb = _SpecColor.rgb;
    #ifdef _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
        sg.a = SAMPLE_TEX2D_SHARP(_MainTex, uv).a * _GlossMapScale;
    #else
        sg.a = _Glossiness;
    #endif
#endif
    return sg;
}

half2 MetallicGloss(float2 uv)
{
    half2 mg;

#ifdef _METALLICGLOSSMAP
    #ifdef _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
        mg.r = SAMPLE_TEX2D_SHARP(_MetallicGlossMap, uv).r;
        mg.g = SAMPLE_TEX2D_SHARP(_MainTex, uv).a;
    #else
        mg = SAMPLE_TEX2D_SHARP(_MetallicGlossMap, uv).ra;
    #endif
    mg.g *= _GlossMapScale;
#else
    mg.r = _Metallic;
    #ifdef _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
        mg.g = SAMPLE_TEX2D_SHARP(_MainTex, uv).a * _GlossMapScale;
    #else
        mg.g = _Glossiness;
    #endif
#endif
    return mg;
}

half2 MetallicRough(float2 uv)
{
    half2 mg;
#ifdef _METALLICGLOSSMAP
    mg.r = SAMPLE_TEX2D_SHARP(_MetallicGlossMap, uv).r;
#else
    mg.r = _Metallic;
#endif

#ifdef _SPECGLOSSMAP
    mg.g = 1.0f - SAMPLE_TEX2D_SHARP(_SpecGlossMap, uv).r;
#else
    mg.g = 1.0f - _Glossiness;
#endif
    return mg;
}

half3 Emission(float2 uv)
{
#ifndef _EMISSION
    return 0;
#else
    return SAMPLE_TEX2D_SHARP(_EmissionMap, uv).rgb * _EmissionColor.rgb;
#endif
}

#ifdef _NORMALMAP
half3 NormalInTangentSpace(float4 texcoords)
{
    half3 normalTangent = UnpackScaleNormal(SAMPLE_TEX2D_SHARP (_BumpMap, texcoords.xy), _BumpScale);

#if _DETAIL && defined(UNITY_ENABLE_DETAIL_NORMALMAP)
    half mask = DetailMask(texcoords.xy);
    half3 detailNormalTangent = UnpackScaleNormal(SAMPLE_TEX2D_SHARP (_DetailNormalMap, texcoords.zw), _DetailNormalMapScale);
    #if _DETAIL_LERP
        normalTangent = lerp(
            normalTangent,
            detailNormalTangent,
            mask);
    #else
        normalTangent = lerp(
            normalTangent,
            BlendNormals(normalTangent, detailNormalTangent),
            mask);
    #endif
#endif

    return normalTangent;
}
#endif

float4 Parallax (float4 texcoords, half3 viewDir)
{
#if !defined(_PARALLAXMAP) || (SHADER_TARGET < 30)
    // Disable parallax on pre-SM3.0 shader target models
    return texcoords;
#else
    half h = SAMPLE_TEX2D_SHARP (_ParallaxMap, texcoords.xy).g;
    float2 offset = ParallaxOffset1Step (h, _Parallax, viewDir);
    return float4(texcoords.xy + offset, texcoords.zw + offset);
#endif

}

#endif // UNITY_STANDARD_INPUT_INCLUDED
