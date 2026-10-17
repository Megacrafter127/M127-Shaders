using UnityEngine;
using UnityEditor;

namespace M127 {
    public struct SpecularProps {
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

        public MaterialProperty albedo,
            color,
            specMap,
            specular,
            smoothness,
            smoothness_channel,
            normal,
            normalScale,
            emission,
            emissionColor;

        public static void swapTexture(MaterialProperty a, MaterialProperty b) {
            Texture tbuf = a.textureValue;
            Vector4 vbuf = a.textureScaleAndOffset;
            a.textureValue = b.textureValue;
            a.textureScaleAndOffset = b.textureScaleAndOffset;
            b.textureValue = tbuf;
            b.textureScaleAndOffset = vbuf;
        }

        public static void swapColor(MaterialProperty a, MaterialProperty b) {
            Color cbuf = a.colorValue;
            a.colorValue = b.colorValue;
            b.colorValue = cbuf;
        }

        public static void swapFloat(MaterialProperty a, MaterialProperty b) {
            float fbuf = a.floatValue;
            a.floatValue = b.floatValue;
            b.floatValue = fbuf;
        }

        public static void swapVector(MaterialProperty a, MaterialProperty b) {
            Vector4 vbuf = a.vectorValue;
            a.vectorValue = b.vectorValue;
            b.vectorValue = vbuf;
        }

        public void swap(SpecularProps other) {
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

        public void apply(MaterialEditor editor, out bool cnormal) {
            Material reference = EditorGUILayout.ObjectField("Reference", null, typeof(Material), true) as Material;
            if(reference != null) {
                albedo.textureValue = reference.GetTexture(ALBEDO_NAME);
                albedo.textureScaleAndOffset = reference.GetTextureOffset(ALBEDO_NAME);
                color.colorValue = reference.GetColor(COLOR_NAME);
                specMap.textureValue = reference.GetTexture(SPEC_MAP_NAME);
                specMap.textureScaleAndOffset = reference.GetTextureScale(SPEC_MAP_NAME);
                specular.floatValue = reference.GetFloat(SPEC_NAME);
                smoothness.floatValue = reference.GetFloat(SMOOTHNESS_NAME);
                smoothness_channel.floatValue = reference.GetFloat(SMOOTHNESS_CHANNEL_NAME);
                normal.textureValue = reference.GetTexture(NORMAL_NAME);
                normal.textureScaleAndOffset = reference.GetTextureOffset(NORMAL_NAME);
                normalScale.floatValue = reference.GetFloat(NORMAL_SCALE_NAME);
                emission.textureValue = reference.GetTexture(EMISSION_NAME);
                emission.textureScaleAndOffset = reference.GetTextureScale(EMISSION_NAME);
                emissionColor.colorValue = reference.GetColor(EMISSION_COLOR_NAME);
            }

            bool hasAlbedo = albedo.textureValue != null;
            editor.TexturePropertySingleLine(
                EditorGUIUtility.TrTextContent(albedo.displayName, ALBEDO_TT),
                albedo,
                color
            );
            if(hasAlbedo) {
                albedoFold = EditorGUILayout.Foldout(albedoFold, "Scale Offset");
                if(albedoFold) editor.TextureScaleOffsetProperty(albedo);
                editor.TextureCompatibilityWarning(albedo);
            }

            bool hasSpecular = specMap.textureValue != null;
            editor.TexturePropertySingleLine(
                EditorGUIUtility.TrTextContent(specMap.displayName, SPECULAR_TT),
                specMap,
                specular
            );
            editor.RangeProperty(smoothness, smoothness.displayName);
            if(hasSpecular) {
                specFold = EditorGUILayout.Foldout(specFold, "Scale Offset");
                if(specFold) editor.TextureScaleOffsetProperty(specMap);
                editor.TextureCompatibilityWarning(specular);
            }

            editor.ShaderProperty(
                smoothness_channel,
                EditorGUIUtility.TrTextContent(
                    smoothness_channel.displayName,
                    "If smoothness should be sourced from albedo alpha instead."
                )
            );
            bool hasNormal = normal.textureValue != null;
            EditorGUI.BeginChangeCheck();
            editor.TexturePropertySingleLine(
                EditorGUIUtility.TrTextContent(normal.displayName, NORMAL_TT),
                normal,
                hasNormal ? normalScale : null
            );
            cnormal = EditorGUI.EndChangeCheck();
            if(hasNormal) {
                normalFold = EditorGUILayout.Foldout(normalFold, "Scale Offset");
                if(normalFold) editor.TextureScaleOffsetProperty(normal);
                editor.TextureCompatibilityWarning(normal);
            }

            bool hasEmission = emission.textureValue != null;
            editor.TexturePropertyWithHDRColor(
                EditorGUIUtility.TrTextContent(emission.displayName, EMISSION_TT),
                emission,
                emissionColor,
                false
            );
            if(hasEmission) {
                emissionFold = EditorGUILayout.Foldout(emissionFold, "Scale Offset");
                if(emissionFold) editor.TextureScaleOffsetProperty(emission);
                editor.TextureCompatibilityWarning(emission);
            }
        }

        public bool absorbProperty(MaterialProperty prop, string suffix) {
            if(!prop.name.EndsWith(suffix)) return false;
            switch(prop.name.Substring(0, prop.name.Length - suffix.Length)) {
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
}