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
// Assets/Editor/ScriptUsageFinderFull.cs
// Assets/Editor/AdvancedScriptFinder.cs
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEditor.Experimental.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class ScriptUsageFinder : EditorWindow
{
    private MonoScript targetScript;
    private Vector2 scrollPos;

    private Dictionary<string, List<ResultItem>> groupedResults = new Dictionary<string, List<ResultItem>>();

    private GUIStyle folderStyle;
    private GUIStyle fileStyle;
    private GUIStyle componentStyle;

    private Dictionary<string, bool> treeFoldout = new Dictionary<string, bool>();

    // GUID tabanlı eşleştirme
    private string targetScriptGuid;

    [MenuItem("Tools/Script Usage Finder")]
    public static void ShowWindow()
    {
        GetWindow<ScriptUsageFinder>("Script Usage Finder");
    }

    private void OnEnable()
    {
        folderStyle = new GUIStyle(EditorStyles.foldout)
        {
            fontStyle = FontStyle.Bold,
            fontSize = 13
        };

        fileStyle = new GUIStyle(GUI.skin.button)
        {
            alignment = TextAnchor.MiddleLeft,
            fontSize = 12
        };

        componentStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 11,
            richText = true
        };
    }

    private void OnGUI()
    {
        GUILayout.Space(10);
        targetScript = (MonoScript)EditorGUILayout.ObjectField("Script", targetScript, typeof(MonoScript), false);

        if (GUILayout.Button("Scan Usage", GUILayout.Height(30)))
        {
            if (targetScript != null)
            {
                string guidCopy = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(targetScript));

                // GUI event'i dışında çalıştır
                EditorApplication.delayCall += () =>
                {
                    ScanProjectForScript(targetScript);
                };
            }
        }


        GUILayout.Space(20);

        scrollPos = GUILayout.BeginScrollView(scrollPos);

        foreach (var group in groupedResults)
        {
            if (!treeFoldout.ContainsKey(group.Key))
                treeFoldout[group.Key] = true;

            treeFoldout[group.Key] = EditorGUILayout.Foldout(treeFoldout[group.Key], group.Key, folderStyle);

            if (treeFoldout[group.Key])
            {
                foreach (var r in group.Value)
                {
                    if (GUILayout.Button(r.displayName, fileStyle))
                    {
                        PingObject(r);
                    }

                    if (r.componentName != null)
                    {
                        GUILayout.Label($"   <color=#888>Component:</color> {r.componentName}", componentStyle);
                    }
                }
            }
        }

        GUILayout.EndScrollView();
    }

    void ScanProjectForScript(MonoScript script)
    {
        groupedResults.Clear();

        string[] allAssets = AssetDatabase.GetAllAssetPaths();
        System.Type scriptType = script.GetClass();
        targetScriptGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(script));

        foreach (string path in allAssets)
        {
            if (path.EndsWith(".prefab"))
                ScanPrefab(path, scriptType);

            if (path.EndsWith(".unity"))
                ScanScene(path, scriptType);
        }

        Repaint();
    }

    // -------------------------
    // PREFAB TARAMA
    // -------------------------
    void ScanPrefab(string path, System.Type scriptType)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null) return;

        var all = prefab.GetComponentsInChildren(scriptType, true);
        foreach (var comp in all)
        {
            var go = ((Component)comp).gameObject;
            AddResult("Prefabs", go.name + "   (" + Path.GetFileName(path) + ")", go, comp.GetType().Name, path);
        }
    }

    // -------------------------
    // SCENE TARAMA
    // -------------------------
    void ScanScene(string path, System.Type scriptType)
    {
        // Package içi sahneleri tarama!
        if (path.StartsWith("Packages/"))
            return;

        Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
        var roots = scene.GetRootGameObjects();

        foreach (var root in roots)
        {
            var comps = root.GetComponentsInChildren(scriptType, true);
            foreach (var comp in comps)
            {
                var go = ((Component)comp).gameObject;
                AddResult("Scenes", go.name + "   (" + Path.GetFileName(path) + ")", go, comp.GetType().Name, path);
            }
        }

        EditorSceneManager.CloseScene(scene, true);
    }


    // -------------------------
    // KAYIT EKLEME
    // -------------------------
    void AddResult(string group, string name, GameObject go, string componentName, string assetPath)
    {
        if (!groupedResults.ContainsKey(group))
            groupedResults[group] = new List<ResultItem>();

        groupedResults[group].Add(new ResultItem
        {
            displayName = name,
            foundObj = go,
            componentName = componentName,
            assetPath = assetPath,
            type = group
        });
    }

    // -------------------------
    // OBJEYİ SEÇME + PREFAB AUTOPEN
    // -------------------------
    void PingObject(ResultItem r)
    {
        // PREFAB
        if (r.type == "Prefabs")
        {
            var stage = PrefabStageUtility.OpenPrefab(r.assetPath);
            var root = stage.prefabContentsRoot;

            var all = root.GetComponentsInChildren<Transform>(true);
            foreach (var t in all)
            {
                if (t.name == r.foundObj.name)
                {
                    Selection.activeObject = t.gameObject;
                    EditorGUIUtility.PingObject(t.gameObject);
                    return;
                }
            }
            return;
        }

        // SCENE
        Selection.activeObject = r.foundObj;
        EditorGUIUtility.PingObject(r.foundObj);
    }

    // -------------------------
    // RESULT ITEM STRUCT
    // -------------------------
    class ResultItem
    {
        public string displayName;
        public string componentName;
        public GameObject foundObj;
        public string assetPath;
        public string type;
    }
}


