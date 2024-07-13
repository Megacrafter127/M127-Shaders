using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Unity.EditorCoroutines.Editor;

public class MaterialCleaner : EditorWindow
{
    [MenuItem("Tools/M127/MaterialCleaner")]
    public static void Open()
    {
        MaterialCleaner cleaner = GetWindow<MaterialCleaner>();
        cleaner.ShowTab();
    }

    private static T ObjectField<T>(T obj) where T : Object
    {
        return (T)EditorGUILayout.ObjectField(obj, typeof(T), true);
    }

    private ISet<Material> materials = new HashSet<Material>();
    private GameObject gameObject;
    private Vector2 scrollPosition;
    private bool busy = false;
    private float progress = 0;

    public void OnGUI()
    {
        EditorGUI.BeginDisabledGroup(busy);
        gameObject = ObjectField(gameObject);
        if(GUILayout.Button("Add Materials") && gameObject is not null) {
            foreach(Renderer renderer in gameObject.GetComponentsInChildren<Renderer>(true)) {
                materials.UnionWith(renderer.sharedMaterials);
            }
            materials.Remove(null);
        }
        if(GUILayout.Button("Add All Materials in Assets")) {
            busy = true;
            EditorCoroutineUtility.StartCoroutine(AddAssetMaterials(), this);
        }
        ISet<Material> removed = new HashSet<Material>();
        ISet<Material> added = new HashSet<Material>();
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        foreach (Material material in materials) {
            Material delta = ObjectField(material);
            if(delta != material) {
                removed.Add(material);
                if (delta is not null) added.Add(delta);
            }
        }
        Material newMaterial = ObjectField<Material>(null);
        GUILayout.EndScrollView();
        if (newMaterial is not null) added.Add(newMaterial);
        materials.ExceptWith(removed);
        materials.UnionWith(added);
        if(GUILayout.Button("Clean")) {
            Material[] undos = new Material[materials.Count];
            materials.CopyTo(undos, 0);
            Undo.RecordObjects(undos, "Clean Properties");
            EditorCoroutineUtility.StartCoroutine(Clean(), this);
        }
        EditorGUI.EndDisabledGroup();
        Rect r = EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("");
        EditorGUI.ProgressBar(r, progress, "");
        EditorGUILayout.EndVertical();
    }

    private IEnumerator AddAssetMaterials()
    {
        string[] guids = AssetDatabase.FindAssets("t:Material", new string[] { "Assets/" });
        int prog = 0;
        foreach (string str in guids) {
            progress = (float)prog++ / guids.Length;
            yield return materials.Add(AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(str)));
        }
        progress = 0;
        busy = false;
    }

    private IEnumerator Clean()
    {
        int prog = 0;
        foreach(Material material in materials) {
            Shader shader = material.shader;
            ISet<int> textureProperties = new HashSet<int>();
            for(int i = 0; i<shader.GetPropertyCount(); i++) {
                if(shader.GetPropertyType(i) == UnityEngine.Rendering.ShaderPropertyType.Texture) {
                    textureProperties.Add(shader.GetPropertyNameId(i));
                }
            }
            foreach(int id in material.GetTexturePropertyNameIDs()) {
                if (textureProperties.Contains(id)) continue;
                material.SetTexture(id, null);
            }
            progress = (float)prog++ / materials.Count;
            yield return material;
        }
        materials.Clear();
        busy = false;
        progress = 0;
    }
}
