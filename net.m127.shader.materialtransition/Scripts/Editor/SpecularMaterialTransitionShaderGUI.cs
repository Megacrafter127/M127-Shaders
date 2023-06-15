using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace M127
{
    public class SpecularMaterialTransitionShaderGUI : ShaderGUI
    {
        public struct StandardProps
        {
            public const string ALBEDO_NAME = "_MainTex",
                COLOR_NAME = "_Color",
                SPEC_MAP_NAME = "_SpecGlossMap",
                SPEC_NAME = "_SpecColor",
                SMOOTHNESS_NAME = "_Glossiness",
                SMOOTHNESS_CHANNEL_NAME = "_SmoothnessTextureChannel",
                NORMAL_NAME = "_BumpMap",
                NORMAL_SCALE_NAME = "_BumpScale",
                EMISSION_NAME = "_EmissionMap",
                EMISSION_COLOR_NAME = "_EmissionColor";

            public const string ALBEDO_TT = "Color of the surface",
                SPECULAR_TT = "Specular Texture",
                NORMAL_TT = "Normal Map",
                EMISSION_TT = "Emission map";

            public MaterialProperty albedo, color, specMap, specular, smoothness, smoothness_channel, normal, normalScale, emission, emissionColor;

            public static void swapTexture(MaterialProperty a, MaterialProperty b)
            {
                Texture tbuf = a.textureValue;
                Vector4 vbuf = a.textureScaleAndOffset;
                a.textureValue = b.textureValue;
                a.textureScaleAndOffset = b.textureScaleAndOffset;
                b.textureValue = tbuf;
                b.textureScaleAndOffset = vbuf;
            }

            public static void swapColor(MaterialProperty a, MaterialProperty b)
            {
                Color cbuf = a.colorValue;
                a.colorValue = b.colorValue;
                b.colorValue = cbuf;
            }

            public static void swapFloat(MaterialProperty a, MaterialProperty b)
            {
                float fbuf = a.floatValue;
                a.floatValue = b.floatValue;
                b.floatValue = fbuf;
            }

            public static void swapVector(MaterialProperty a, MaterialProperty b)
            {
                Vector4 vbuf = a.vectorValue;
                a.vectorValue = b.vectorValue;
                b.vectorValue = vbuf;
            }

            public void swap(StandardProps other)
            {
                swapTexture(albedo, other.albedo);
                swapColor(color, other.color);
                swapTexture(specMap, other.specMap);
                swapColor(specular, other.specular);
                swapFloat(smoothness, other.smoothness);
                swapTexture(normal, other.normal);
                swapFloat(normalScale, other.normalScale);
                swapTexture(emission, other.emission);
                swapColor(emissionColor, other.emissionColor);
            }

            private bool albedoFold, specFold, normalFold, emissionFold;
            public void apply(MaterialEditor editor, out bool cnormal)
            {
                bool hasAlbedo = albedo.textureValue != null;
                editor.TexturePropertySingleLine(EditorGUIUtility.TrTextContent(albedo.displayName, ALBEDO_TT), albedo, color);
                if (hasAlbedo)
                {
                    albedoFold = EditorGUILayout.Foldout(albedoFold, "Scale Offset");
                    if (albedoFold) editor.TextureScaleOffsetProperty(albedo);
                    editor.TextureCompatibilityWarning(albedo);
                }
                bool hasSpecular = specMap.textureValue != null;
                editor.TexturePropertySingleLine(EditorGUIUtility.TrTextContent(specMap.displayName, SPECULAR_TT), specMap, specular);
                editor.RangeProperty(smoothness, smoothness.displayName);
                if (hasSpecular)
                {
                    specFold = EditorGUILayout.Foldout(specFold, "Scale Offset");
                    if (specFold) editor.TextureScaleOffsetProperty(specMap);
                    editor.TextureCompatibilityWarning(specular);
                }
                editor.ShaderProperty(smoothness_channel, EditorGUIUtility.TrTextContent(smoothness_channel.displayName, "If smoothness should be sourced from albedo alpha instead."));
                bool hasNormal = normal.textureValue != null;
                EditorGUI.BeginChangeCheck();
                editor.TexturePropertySingleLine(EditorGUIUtility.TrTextContent(normal.displayName, NORMAL_TT), normal, hasNormal ? normalScale : null);
                cnormal = EditorGUI.EndChangeCheck();
                if (hasNormal)
                {
                    normalFold = EditorGUILayout.Foldout(normalFold, "Scale Offset");
                    if (normalFold) editor.TextureScaleOffsetProperty(normal);
                    editor.TextureCompatibilityWarning(normal);
                }
                bool hasEmission = emission.textureValue != null;
                editor.TexturePropertyWithHDRColor(EditorGUIUtility.TrTextContent(emission.displayName, EMISSION_TT), emission, emissionColor, false);
                if (hasEmission)
                {
                    emissionFold = EditorGUILayout.Foldout(emissionFold, "Scale Offset");
                    if (emissionFold) editor.TextureScaleOffsetProperty(emission);
                    editor.TextureCompatibilityWarning(emission);
                }
            }
            public bool absorbProperty(MaterialProperty prop, string suffix)
            {
                if (!prop.name.EndsWith(suffix)) return false;
                switch (prop.name.Substring(0, prop.name.Length - suffix.Length))
                {
                    case ALBEDO_NAME:
                        albedo = prop;
                        break;
                    case COLOR_NAME:
                        color = prop;
                        break;
                    case SPEC_MAP_NAME:
                        specMap = prop;
                        break;
                    case SPEC_NAME:
                        specular = prop;
                        break;
                    case SMOOTHNESS_NAME:
                        smoothness = prop;
                        break;
                    case SMOOTHNESS_CHANNEL_NAME:
                        smoothness_channel = prop;
                        break;
                    case NORMAL_NAME:
                        normal = prop;
                        break;
                    case NORMAL_SCALE_NAME:
                        normalScale = prop;
                        break;
                    case EMISSION_NAME:
                        emission = prop;
                        break;
                    case EMISSION_COLOR_NAME:
                        emissionColor = prop;
                        break;
                    default:
                        return false;
                }
                return true;
            }
        }

        public bool m1f, m2f, sf, tf, pf, nf;
        public const string CUTOFF_NAME = "_Cutoff",
            OFFSET_NAME = "_Offset",
            BBMIN_NAME = "_BoundingBoxMin",
            BBMAX_NAME = "_BoundingBoxMax",
            NOISE_NAME = "_Noise",
            NOISE_SCALE_NAME = "_NoiseStrength",
            SOURCE_NAME = "_SourceVector",
            SOURCE_TYPE_NAME = "_Source_Type",
            SHIFT_NAME = "_Shift",
            COMPLETION_NAME = "_Completion";

        public StandardProps m1 = new StandardProps(), m2 = new StandardProps();

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            MaterialProperty cutoff = null, offset = null, bbmin = null, bbmax = null, noise = null, noiseScale = null, source = null, sourceType = null, shift = null, completion = null;
            foreach (MaterialProperty p in properties)
            {
                if (!m1.absorbProperty(p, "") && !m2.absorbProperty(p, "2"))
                {
                    switch (p.name)
                    {
                        case CUTOFF_NAME:
                            cutoff = p;
                            break;
                        case OFFSET_NAME:
                            offset = p;
                            break;
                        case BBMIN_NAME:
                            bbmin = p;
                            break;
                        case BBMAX_NAME:
                            bbmax = p;
                            break;
                        case NOISE_NAME:
                            noise = p;
                            break;
                        case NOISE_SCALE_NAME:
                            noiseScale = p;
                            break;
                        case SOURCE_NAME:
                            source = p;
                            break;
                        case SOURCE_TYPE_NAME:
                            sourceType = p;
                            break;
                        case SHIFT_NAME:
                            shift = p;
                            break;
                        case COMPLETION_NAME:
                            completion = p;
                            break;
                        default:
                            throw new ArgumentException("Unknown MaterialProperty: " + p.name);
                    }
                }
            }
            Material mat = materialEditor.target as Material;
            m1f = EditorGUILayout.BeginFoldoutHeaderGroup(m1f, "Destination Material");
            bool normal1 = false, normal2 = false;
            if (m1f) m1.apply(materialEditor, out normal1);
            EditorGUILayout.EndFoldoutHeaderGroup();
            m2f = EditorGUILayout.BeginFoldoutHeaderGroup(m2f, "Source Material");
            if (m2f) m2.apply(materialEditor, out normal2);
            EditorGUILayout.EndFoldoutHeaderGroup();
            if (GUILayout.Button("Swap Source and Destination"))
            {
                m1.swap(m2);
            }
            if (cutoff != null)
            {
                sf = EditorGUILayout.BeginFoldoutHeaderGroup(sf, "Shared Material Settings");
                if (sf)
                {
                    materialEditor.RangeProperty(cutoff, cutoff.displayName);
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
            }
            tf = EditorGUILayout.BeginFoldoutHeaderGroup(tf, "Transition Settings");
            if (tf)
            {
                materialEditor.VectorProperty(offset, offset.displayName);
                materialEditor.VectorProperty(bbmin, bbmin.displayName);
                materialEditor.VectorProperty(bbmax, bbmax.displayName);
                bool hasNoise = noise.textureValue != null;
                materialEditor.TexturePropertySingleLine(EditorGUIUtility.TrTextContent(noise.displayName, "UV-mapped distorted perlin noise"), noise);
                if (hasNoise)
                {
                    nf = EditorGUILayout.Foldout(nf, "Scale Offset");
                    if (nf) materialEditor.TextureScaleOffsetProperty(noise);
                    materialEditor.RangeProperty(noiseScale, noiseScale.displayName);
                }
                else noiseScale.floatValue = 0;
                GUIContent sourceDesc;
                if (sourceType.floatValue > .5)
                {
                    sourceDesc = EditorGUIUtility.TrTextContent(source.displayName, "Direction in which the transformation travels");
                }
                else
                {
                    sourceDesc = EditorGUIUtility.TrTextContent(source.displayName, "Origin coordinates of transformation normalized within mesh bounding box");
                }
                materialEditor.ShaderProperty(source, sourceDesc);
                materialEditor.ShaderProperty(sourceType, sourceType.displayName);

                materialEditor.RangeProperty(shift, shift.displayName);
                materialEditor.RangeProperty(completion, completion.displayName);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            materialEditor.RenderQueueField();
            if (normal1 || normal2)
            {
                if (m1.normal.textureValue == null && m2.normal.textureValue == null)
                {
                    mat.DisableKeyword("_NORMALMAP");
                }
                else
                {
                    mat.EnableKeyword("_NORMALMAP");
                }
            }
        }
    }
}