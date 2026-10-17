using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace M127
{
    public class SpecularMaterialTransitionShaderGUI2 : ShaderGUI
    {
        public bool m1f, m2f, m3f, sf, tf, pf, nf;
        public const string CUTOFF_NAME = "_Cutoff",
            OFFSET_NAME = "_Offset",
            BBMIN_NAME = "_BoundingBoxMin",
            BBMAX_NAME = "_BoundingBoxMax",
            NOISE_NAME = "_Noise",
            NOISE_SCALE_NAME = "_NoiseStrength",
            SOURCE_NAME = "_SourceVector",
            SOURCE_TYPE_NAME = "_Source_Type",
            SHIFT_NAME = "_Shift",
            COMPLETION_NAME = "_Completion",
            COMPLETION2_NAME = "_Completion2";

        public SpecularProps m1 = new(), m2 = new(), m3 = new();

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            MaterialProperty cutoff = null, offset = null, bbmin = null, bbmax = null, noise = null, noiseScale = null, source = null, sourceType = null, shift = null, completion = null, completion2 = null;
            foreach (MaterialProperty p in properties)
            {
                if (!m1.absorbProperty(p, "") && !m2.absorbProperty(p, "2") && !m3.absorbProperty(p, "3"))
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
                        case COMPLETION2_NAME:
                            completion2 = p;
                            break;
                        default:
                            throw new ArgumentException("Unknown MaterialProperty: " + p.name);
                    }
                }
            }
            Material mat = materialEditor.target as Material;
            m1f = EditorGUILayout.BeginFoldoutHeaderGroup(m1f, "Destination Material");
            bool normal1 = false, normal2 = false, normal3 = false;
            if (m1f) m1.apply(materialEditor, out normal1);
            EditorGUILayout.EndFoldoutHeaderGroup();
            m2f = EditorGUILayout.BeginFoldoutHeaderGroup(m2f, "Transition Material");
            if (m2f) m2.apply(materialEditor, out normal2);
            EditorGUILayout.EndFoldoutHeaderGroup();
            m3f = EditorGUILayout.BeginFoldoutHeaderGroup(m3f, "Source Material");
            if (m3f) m2.apply(materialEditor, out normal3);
            EditorGUILayout.EndFoldoutHeaderGroup();
            if (GUILayout.Button("Swap Destionation and Transition"))
            {
                m1.swap(m2);
            }
            if (GUILayout.Button("Swap Source and Destination"))
            {
                m1.swap(m3);
            }
            if (GUILayout.Button("Swap Source and Transition"))
            {
                m2.swap(m3);
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
                materialEditor.RangeProperty(completion2, completion2.displayName);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            materialEditor.RenderQueueField();
            if (normal1 || normal2 || normal3)
            {
                if (m1.normal.textureValue == null && m2.normal.textureValue == null && m3.normal.textureValue == null)
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