// Assets/Editor/ScriptUsageFinder.cs - Hata Düzeltme Versiyonu
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Object = UnityEngine.Object;

public class ScriptUsageFinder : EditorWindow
{
    private MonoScript targetScript;
    private Vector2 scrollPos;
    private Dictionary<string, List<ResultItem>> groupedResults = new Dictionary<string, List<ResultItem>>();
    
    private GUIStyle folderStyle;
    private GUIStyle fileStyle;
    private GUIStyle componentStyle;
    
    private Dictionary<string, bool> treeFoldout = new Dictionary<string, bool>();
    
    private string targetScriptGuid;
    private System.Type targetScriptType; // Component araması için Type bilgisi
    
    [MenuItem("Tools/Find Script Usage (Deep Fix)")]
    public static void ShowWindow()
    {
        GetWindow<ScriptUsageFinder>("Script Usage Finder");
    }

    private void OnEnable()
    {
        // Stillerin tanımlanması
        folderStyle = new GUIStyle(EditorStyles.foldout) { fontStyle = FontStyle.Bold, fontSize = 13 };
        fileStyle = new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleLeft, fontSize = 12 };
        componentStyle = new GUIStyle(GUI.skin.label) { fontSize = 11, richText = true };
    }

    private void OnGUI()
    {
        // ... (GUI kısmı aynı kalabilir)
        GUILayout.Space(10);
        targetScript = (MonoScript)EditorGUILayout.ObjectField("Script", targetScript, typeof(MonoScript), false);

        if (GUILayout.Button("Scan Usage in Project (Fixed Deep Search)", GUILayout.Height(30)))
        {
            if (targetScript != null)
            {
                EditorApplication.delayCall += () =>
                {
                    ScanProjectForScript(targetScript);
                };
            }
        }

        GUILayout.Space(20);
        scrollPos = GUILayout.BeginScrollView(scrollPos);
        DrawResults();
        GUILayout.EndScrollView();
    }

    // -------------------------
    // TARAMA İŞLEMİ
    // -------------------------
    void ScanProjectForScript(MonoScript script)
    {
        groupedResults.Clear();
        targetScriptGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(script));
        targetScriptType = script.GetClass(); 

        if (string.IsNullOrEmpty(targetScriptGuid) || targetScriptType == null) 
        {
            Debug.LogError("Selected script is invalid or not a MonoScript/Component.");
            return;
        }

        string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();
        int count = 0;
        int total = allAssetPaths.Length;
        // Unity asset formatında arama dizesi
        string guidSearchPattern = $"guid: {targetScriptGuid}, type: 3"; 

        foreach (string path in allAssetPaths)
        {
            count++;
            EditorUtility.DisplayProgressBar("Scanning Project...", path, (float)count / total);

            if (path.EndsWith(".cs") || path.StartsWith("Packages/") || path.Contains("/Library/"))
                continue;

            if (IsTextAsset(path))
            {
                ScanAssetForGUIDAndDeepCheck(path, guidSearchPattern);
            }
        }

        EditorUtility.ClearProgressBar();
        Repaint();
        Debug.Log($"Scan complete. Found {groupedResults.Values.Sum(l => l.Count)} usages.");
    }

    // Hangi dosyaların metin olarak okunacağını belirler
    bool IsTextAsset(string path)
    {
        string ext = Path.GetExtension(path).ToLower();
        return ext == ".prefab" || ext == ".unity" || ext == ".asset" || 
               ext == ".mat" || ext == ".anim" || ext == ".mixer";
    }

    // Dosyayı metin olarak okuyup GUID arar ve derin taramaya yönlendirir
    void ScanAssetForGUIDAndDeepCheck(string path, string searchPattern)
    {
        try
        {
            string content = File.ReadAllText(path);
            
            if (content.Contains(searchPattern))
            {
                string ext = Path.GetExtension(path).ToLower();
                
                if (ext == ".prefab")
                    ScanPrefabDeep(path); // Prefab'i açıp GameObject/Component bul
                else if (ext == ".unity")
                    ScanSceneDeep(path); // Scene'i açıp GameObject/Component bul
                else
                {
                    // Diğer asset türleri (ScriptableObjects, Materials vb.)
                    AddResult("3. Other Assets", Path.GetFileName(path), null, "Asset Reference", path);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error reading asset at {path}: {e.Message}");
        }
    }

    // -------------------------
    // PREFAB DERİN TARAMA (Düzeltildi)
    // -------------------------
    void ScanPrefabDeep(string path)
    {
        // AssetDatabase.LoadAssetAtPath yerine Prefab StageUtility ile açıp tarayalım,
        // bu en güvenilir yöntemdir, ancak editör modunda olduğu için karmaşık.
        // Hata ayıklaması için daha basit ve genellikle güvenilir olan LoadAssetAtPath'i geri getiriyoruz.
        
        GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefabAsset == null) return;

        // Prefab içindeki tüm component'leri tarar (targetScriptType'a sahip olanları)
        // Eğer script bir Component değilse GetComponentsInChildren boş dönecektir.
        var allComponents = prefabAsset.GetComponentsInChildren(targetScriptType, true);

        // Hata: Script MonoBehavior değil, ScriptableObject olabilir. 
        // Ancak bu araç MonoScript'ler için tasarlandığı için Component/MonoBehaviour kabul ediyoruz.

        if (allComponents.Length > 0)
        {
            foreach (var comp in allComponents)
            {
                var go = ((Component)comp).gameObject;
                string group = "1. Prefabs (GameObjects)";
                // Prefab'de referans geçersiz olma riski azdır.
                AddResult(group, go.name + "   (" + Path.GetFileName(path) + ")", go, comp.GetType().Name, path);
            }
        }
        else
        {
            // Eğer script MonoBehaviour değilse ve sadece bir ScriptableObject ise,
            // sadece GUID kontrolü yeterlidir. Bu, Prefab içindeki bir asset referansı olabilir.
            AddResult("1. Prefabs (Asset Ref)", Path.GetFileName(path), null, "Internal Asset Reference", path);
        }
    }

    // -------------------------
    // SCENE DERİN TARAMA (Düzeltildi)
    // -------------------------
    void ScanSceneDeep(string path)
    {
        // Sahneyi additive modda aç
        var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
        
        try
        {
            var roots = scene.GetRootGameObjects();

            // Scene içindeki tüm objeleri tarar
            foreach (var root in roots)
            {
                var comps = root.GetComponentsInChildren(targetScriptType, true);
                
                if (comps.Length > 0)
                {
                    foreach (var comp in comps)
                    {
                        string componentName = comp.GetType().Name;
                        string goName = comp.gameObject.name;
                        
                        // Scene objesi geçersiz olacağı için 'foundObj' null bırakılır.
                        AddResult("2. Scenes (GameObjects)", goName + "   (" + Path.GetFileName(path) + ")", null, componentName, path);
                    }
                }
                // ScriptableObject vb. Scene içinde direkt Component olarak bulunmaz, 
                // bu yüzden sadece Component bulma üzerine odaklanıyoruz.
            }
        }
        finally
        {
            // Mutlaka sahneyi kapat!
            EditorSceneManager.CloseScene(scene, true);
        }
    }
    
    // -------------------------
    // OBJEYİ SEÇME (PING)
    // -------------------------
    void PingObject(ResultItem r)
    {
        // 1. Scene Kullanımı
        if (r.type.Contains("Scenes"))
        {
            // Scene dosyasını seç ve kullanıcıyı uyarma
            Object sceneAsset = AssetDatabase.LoadAssetAtPath<Object>(r.assetPath);
            if (sceneAsset != null)
            {
                Selection.activeObject = sceneAsset;
                EditorGUIUtility.PingObject(sceneAsset);
                Debug.LogWarning($"Usage found in Scene: {Path.GetFileName(r.assetPath)}. To locate the GameObject, open the scene and search for '{r.displayName.Split('(')[0].Trim()}' and Component '{r.componentName}'.");
            }
            return;
        }

        // 2. Prefab Kullanımı (Prefab Stage'i açıp objeyi seç)
        if (r.type.Contains("Prefabs") && r.foundObj != null)
        {
            // Prefab Stage'i aç
            var stage = UnityEditor.SceneManagement.PrefabStageUtility.OpenPrefab(r.assetPath);
            if (stage == null) return;
            var root = stage.prefabContentsRoot;

            // Objeyi isme ve component tipine göre bulmaya çalış
            var allComponents = root.GetComponentsInChildren(targetScriptType, true);
            foreach (var comp in allComponents)
            {
                // DisplayName içindeki GameObject adını ayır
                string goName = r.displayName.Split('(')[0].Trim();

                if (comp.gameObject.name == goName && comp.GetType().Name == r.componentName)
                {
                    Selection.activeObject = comp.gameObject;
                    EditorGUIUtility.PingObject(comp.gameObject);
                    return;
                }
            }
            
            // Eğer objeyi bulamazsa (isim değiştiyse vb.), Prefab asset'ini seç
            Object prefabAsset = AssetDatabase.LoadAssetAtPath<Object>(r.assetPath);
            Selection.activeObject = prefabAsset;
            EditorGUIUtility.PingObject(prefabAsset);
            return;
        }

        // 3. Diğer Asset Kullanımı
        Object asset = AssetDatabase.LoadAssetAtPath<Object>(r.assetPath);
        if (asset != null)
        {
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }
    }
    
    // -------------------------
    // DİĞER METOTLAR
    // -------------------------

    // Sonuçları gösterme/çizme (Aynı kalır)
    void DrawResults()
    {
        // ... (Kod aynı)
        var sortedGroups = new SortedDictionary<string, List<ResultItem>>(groupedResults);

        if (groupedResults.Count == 0)
        {
            GUILayout.Label(targetScript == null ? "Select a script to start scanning." : "No usage found in project assets.");
        }

        foreach (var group in sortedGroups)
        {
            if (!treeFoldout.ContainsKey(group.Key))
                treeFoldout[group.Key] = true;

            treeFoldout[group.Key] = EditorGUILayout.Foldout(treeFoldout[group.Key], $"{group.Key} ({group.Value.Count})", folderStyle);

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
                        GUILayout.Label($"   <color=#888>Component/Object:</color> {r.componentName}", componentStyle);
                    }
                }
            }
        }
    }
    
    // Kayıt ekleme (Aynı kalır)
    void AddResult(string group, string name, GameObject go, string componentName, string assetPath)
    {
        if (!groupedResults.ContainsKey(group))
            groupedResults[group] = new List<ResultItem>();

        if (groupedResults[group].Any(r => r.assetPath == assetPath && r.displayName == name)) return;

        groupedResults[group].Add(new ResultItem
        {
            displayName = name,
            foundObj = go, 
            componentName = componentName,
            assetPath = assetPath,
            type = group
        });
    }

    // ResultItem struct (Aynı kalır)
    class ResultItem
    {
        public string displayName; 
        public string componentName; 
        public GameObject foundObj; 
        public string assetPath; 
        public string type;
    }
}