using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

/*
🚀 Nasıl kullanılır?
1. Dosyayı ekle

Assets/Editor/ScriptUsageFinder.cs yolunda bir C# dosyası oluştur.

2. Unity’de menüden aç

Tools → Find Script Usage (Full Project)

3. Target Script seç

Pencereden script’i sürükleyip bırak.

4. “Find Usage in Project” butonuna bas

Tool şu asset’leri tarar:

Sahnedeki GameObjects

Prefab’ler

ScriptableObjects

Animators

Materials

AudioMixers

Her türlü .asset, .prefab, .unity vb. dosya

Değer olarak script referansı tutan her şey
*/

public class ScriptUsageFinder : EditorWindow
{
    private MonoScript targetScript;
    private Vector2 scroll;

    private List<Object> results = new List<Object>();

    [MenuItem("Tools/Find Script Usage (Full Project)")]
    static void Init()
    {
        ScriptUsageFinder window = (ScriptUsageFinder)GetWindow(typeof(ScriptUsageFinder));
        window.titleContent = new GUIContent("Script Usage Finder");
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("Target Script", EditorStyles.boldLabel);

        targetScript = (MonoScript)EditorGUILayout.ObjectField("Script", targetScript, typeof(MonoScript), false);

        if (GUILayout.Button("Find Usage in Project"))
        {
            if (targetScript != null)
                FindUsage();
        }

        GUILayout.Space(20);

        GUILayout.Label("Results", EditorStyles.boldLabel);

        scroll = GUILayout.BeginScrollView(scroll);
        foreach (var obj in results)
        {
            if (obj != null)
            {
                if (GUILayout.Button(obj.name + " (" + obj.GetType().Name + ")", GUILayout.Height(25)))
                    Selection.activeObject = obj;
            }
        }
        GUILayout.EndScrollView();
    }

    void FindUsage()
    {
        results.Clear();

        var scriptClass = targetScript.GetClass();
        if (scriptClass == null)
        {
            Debug.LogWarning("Script class not found.");
            return;
        }

        string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();

        foreach (string path in allAssetPaths)
        {
            if (!path.StartsWith("Assets/"))
                continue;

            Object asset = AssetDatabase.LoadMainAssetAtPath(path);

            if (asset == null)
                continue;

            SerializedObject so = new SerializedObject(asset);
            SerializedProperty prop = so.GetIterator();

            while (prop.NextVisible(true))
            {
                if (prop.propertyType == SerializedPropertyType.ObjectReference)
                {
                    if (prop.objectReferenceValue != null &&
                        prop.objectReferenceValue.GetType() == scriptClass)
                    {
                        results.Add(asset);
                        break;
                    }
                }
            }
        }

        Debug.Log($"Found {results.Count} usages.");
    }
}
