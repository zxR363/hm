#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

/// <summary>
/// AUTOMATION TOOL: Character Creation System Setup
/// "Tools > Setup Creation Panel (Refactor)" menüsünden çalıştırılabilir.
/// Sahnede eski scriptleri temizler ve yeni Controller-UI-Modifier yapısını kurar.
/// </summary>
public class CharacterCreationSetupTool : MonoBehaviour
{
    [MenuItem("Tools/Setup Creation Panel (Refactor)")]
    public static void SetupScene()
    {
        Debug.Log("🚀 Starting Setup...");

        // 1. Root'u bul veya oluştur
        GameObject root = GameObject.Find("CharacterCreationPanel");
        if (root == null)
        {
            Debug.LogError("Sahne'de 'CharacterCreationPanel' objesi bulunamadı! Lütfen panelin adını kontrol edin.");
            return;
        }

        // 2. Controller Ekle
        var controller = root.GetComponent<CharacterCreationController>();
        if (controller == null) controller = root.AddComponent<CharacterCreationController>();
        
        // 3. Modifier Ekle
        var modifier = root.GetComponent<CharacterModifier>();
        if (modifier == null) modifier = root.AddComponent<CharacterModifier>();
        controller.modifier = modifier;

        // 4. UI Manager Ekle
        var uiManager = root.GetComponent<CreationUIManager>();
        
        CleanupMissingScripts(root);

        if(uiManager == null) uiManager = root.GetComponent<CreationUIManager>();
        if(uiManager == null) uiManager = root.AddComponent<CreationUIManager>();
        controller.uiManager = uiManager;

        // 5. Referansları Otomatik Bul
        Debug.Log("🔍 Auto-linking references...");
        
        // --- GRIDS ---
        // Category Grid
        Transform catGrid = FindRecursive(root.transform, "SelectionTabs"); 
        if(catGrid == null) catGrid = FindRecursive(root.transform, "CategoryGridParent"); 
        if(catGrid != null) uiManager.categoryTabParent = catGrid;

        // Item Grid
        Transform optionGrid = FindRecursive(root.transform, "OptionGrid");
        if(optionGrid != null) 
        {
             ScrollRect scroll = optionGrid.GetComponent<ScrollRect>(); // Usually on OptionGrid
             
             if(scroll == null && optionGrid.parent != null) scroll = optionGrid.GetComponentInParent<ScrollRect>();
             
             if(scroll != null)
             {
                 uiManager.contentScrollRect = scroll;
                 uiManager.itemGridParent = scroll.content; 
             }
             else
             {
                 uiManager.itemGridParent = FindRecursive(optionGrid, "Content");
             }
        }
        else
        {
             uiManager.itemGridParent = FindRecursive(root.transform, "OptionGridParent");
        }

        // --- PREFABS ---
        // Assets/Resources/Prefabs/Character/...
        GameObject charPrefab = Resources.Load<GameObject>("GeneratedCharacters/BaseCharacterPreFab/BaseCharacterPrefab");
        if(charPrefab == null) charPrefab = Resources.Load<GameObject>("GeneratedCharacters/Character");
        if(charPrefab != null) controller.characterPrefab = charPrefab;

        GameObject catBtnPrefab = Resources.Load<GameObject>("Prefabs/Character/CategoryButtonTemplatePreFab");
        if(catBtnPrefab != null) uiManager.categoryButtonPrefab = catBtnPrefab;

        GameObject optItemPrefab = Resources.Load<GameObject>("Prefabs/Character/OptionItemPreFab");
        if(optItemPrefab != null) uiManager.optionItemPrefab = optItemPrefab;


        // Color Palette
        Transform toneSlider = FindRecursive(root.transform, "ToneSliderArea");
        if(toneSlider != null) uiManager.colorPalettePanel = toneSlider.gameObject;

        // Preview Spawn Point
        Transform preview = FindRecursive(root.transform, "PreviewArea");
        if(preview != null) controller.previewSpawnPoint = preview;

        // --- INTEGRATION: Selection Manager Link ---
        CharacterSelectionManager selMan = Object.FindObjectOfType<CharacterSelectionManager>();
        if(selMan != null)
        {
             selMan.characterCreationController = controller;
             Debug.Log("🔗 Linked SelectionManager to CreationController");
             EditorUtility.SetDirty(selMan);
        }

        EditorUtility.SetDirty(root);
        EditorUtility.SetDirty(controller);
        EditorUtility.SetDirty(uiManager);
        Debug.Log("✅ Setup Complete!");
    }

    private static void CleanupMissingScripts(GameObject go)
    {
        GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
    }
    
    private static Transform FindRecursive(Transform root, string name)
    {
        if (root.name.Contains(name)) return root;
        foreach (Transform child in root)
        {
            var res = FindRecursive(child, name);
            if (res != null) return res;
        }
        return null;
    }
}
#endif
