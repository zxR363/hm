using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;

/// <summary>
/// Character Creation Main ORCHESTRATOR
/// Logiği (Modifier) ve Görseli (UIManager) yönetir.
/// Veriyi (Data) tutar ve dağıtır.
/// </summary>
public class CharacterCreationController : MonoBehaviour
{
    [Header("Managers")]
    public CharacterModifier modifier;
    public CreationUIManager uiManager;
    
    // State Tracking
    private string currentCategory;
    private Color currentBaseColor = Color.white;

    [Header("Configuration")]
    public GameObject characterPrefab;
    public Transform previewSpawnPoint;
    
    [Header("Data & Resources")]
    // Renk listeleri (ScriptableObject veya direkt liste)
    public List<Color> skinColors; 
    public List<Color> hairColors;      
    public List<Color> eyesColors;      
    
    // Runtime State
    public GameObject currentCharacter;

    private void Start()
    {
        // 1. Initialize Components
        if (uiManager != null) 
        {
            uiManager.Initialize();
            // Slider Event Bağlama
            uiManager.BindToneSlider(OnToneSliderChanged);
        }
        
        // 2. Load Resources
        LoadColors();

        // 3. Setup UI Categories
        SetupCategories();
    }

    /// <summary>
    /// Dışarıdan (CharacterSelectionManager) düzenlenecek karakteri atar.
    /// </summary>
    public void SetCurrentCharacter(GameObject character)
    {
        currentCharacter = character;
        // 🔥 Bug Fix (v15): Don't call PreRegister here, it overwrites specific paths with defaults when editing existing characters.
    }

    // 🔥 v28: Ensure we always start from the first category (Skin)
    public void ResetToFirstCategory()
    {
        Debug.Log("[Controller] Resetting to default category: Skin");
        OnCategorySelected("Skin");
    }

    private void PreRegisterInitialPaths()
    {
        string[] parts = { "Skin", "Hair", "Eyes", "EyeBrown", "Noise", "Freckles", "Mouth", "Clothes", "Hat", "Accessory" };
        foreach (string p in parts)
        {
            modifier.RegisterPartPath(p, CharacterModifier.GetDefaultPath(p));
        }
    }

    // Bu metod artik kullanilmiyor olabilir ama referans hatasi vermemesi icin tutuyoruz
    private void SpawnCharacter()
    {
        // 1. PreviewArea altındaki tüm çocukları kontrol et
        int childCount = previewSpawnPoint.childCount;
        
        if (childCount > 0)
        {
            GameObject keeper = null;
            foreach (Transform child in previewSpawnPoint)
            {
                if(keeper == null && child.gameObject.activeSelf) keeper = child.gameObject;
            }
            
            if (keeper == null) keeper = previewSpawnPoint.GetChild(0).gameObject;
            
            currentCharacter = keeper;
            currentCharacter.SetActive(true);
            return;
        }

        if (characterPrefab != null)
        {
            currentCharacter = Instantiate(characterPrefab, previewSpawnPoint);
            currentCharacter.transform.localPosition = Vector3.zero;
            currentCharacter.transform.localRotation = Quaternion.identity;
        }
    }

    private void OnToneSliderChanged(float val)
    {
        // Slider değiştiğinde mevcut rengin tonunu ayarla ve uygula
        if(currentCharacter == null) return;
        
        Color tonedColor = modifier.AdjustColorTone(currentBaseColor, val);
        
        string targetPart = "Skin"; // Default
        if(currentCategory != null)
        {
            if(currentCategory.Contains("Hair") || currentCategory == "Beard") targetPart = "Hair"; 
            else if(currentCategory == "Eyes") targetPart = "Eyes";
            else if(currentCategory == "Beard") targetPart = "Beard";
        }

        modifier.SetBodyPartColor(currentCharacter, targetPart, tonedColor);
    }

    private void LoadColors()
    {
        // 🎯 Production Resource Loading
        skinColors = LoadColorList<SkinColorList>("Images/Character/Style/SkinColors/SkinColorList");
        hairColors = LoadColorList<ItemsColorList>("Images/Character/Style/ItemColors/HairColorList");
        eyesColors = LoadColorList<ItemsColorList>("Images/Character/Style/ItemColors/EyesColorList");
    }
    
    // Generic Helper for custom color list classes
    private List<Color> LoadColorList<T>(string path) where T : class
    {
        var rawAsset = Resources.Load(path); 
        
        if (rawAsset != null)
        {
             try
             {
                 FieldInfo field = rawAsset.GetType().GetField("colors");
                 if (field != null)
                 {
                     return field.GetValue(rawAsset) as List<Color>;
                 }
                 else
                 {
                      Debug.LogWarning($"[Controller] 'colors' field not found in {rawAsset.GetType().Name}");
                 }
             }
             catch (System.Exception e)
             {
                 Debug.LogWarning($"[Controller] Could not read 'colors' from {path}. Error: {e.Message}");
             }
        }
        
        Debug.LogWarning($"[Controller] Resource not found: {path}");
        return new List<Color>() { Color.white }; 
    }

    private void SetupCategories()
    {
        List<string> categories = new List<string>() 
        { 
            "Skin", "BoyHair", "GirlHair", "MixedHair", 
            "Beard", "Eyes", "Eyebrows", "Nose", "Freckles", "Mouth",
            "Clothes", "Hats", "Accessory"
        };

        uiManager.CreateCategoryTabs(categories, (index) => 
        {
             string catName = categories[index];
             OnCategorySelected(catName);
        });
        
        OnCategorySelected("Skin"); 
    }

    private void OnCategorySelected(string category)
    {
        currentCategory = category;
        Debug.Log($"Category Selected: {category}");
        
        // 1. Reset Palette & Clear ALL Grids (Clean Slate)
        uiManager.SetColorPaletteActive(false);
        uiManager.ClearSubCategoryGrid();
        uiManager.ClearItemsGrid();

        // 2. Try Component-Based Logic First
        CategoryTab catTab = FindCategoryTab(category);
        
        // 🔥 NEW: If a manual basePath is provided, load it IMMEDIATELY as the default view.
        if (catTab != null && !string.IsNullOrEmpty(catTab.basePath))
        {
            Debug.Log($"[Controller] Category {category} has basePath: {catTab.basePath}. Pre-loading items.");
            LoadItemsForCategory(category, "", catTab);
        }

        if(catTab != null && catTab.subCategories.Count > 0)
        {
             // Show Sub Categories (Drill Down)
             List<Sprite> icons = new List<Sprite>();
             List<System.Action> actions = new List<System.Action>();
             
             foreach(var sub in catTab.subCategories)
             {
                 icons.Add(sub.icon); // Can be null, grid handles it or we fallback
                 
                 if(sub.type == SubCategoryType.ColorPalette)
                 {
                     List<Color> dynamicColors = null;

                     // 1. Try Dragged ScriptableObject (SubCategory only now)
                     Object targetPalette = sub.paletteObject;
                     
                     if(targetPalette != null)
                     {
                         Debug.Log($"[Controller] Found Palette Object: {targetPalette.name} (Type: {targetPalette.GetType().FullName})");
                         
                         // Strategy 1: Explicit Cast (Robust)
                         if(targetPalette is ItemsColorList itemsList) 
                         {
                             dynamicColors = itemsList.colors;
                             Debug.Log($"[Controller] Cast to ItemsColorList Success. Count: {(dynamicColors?.Count ?? 0)}");
                         }
                         else if(targetPalette is SkinColorList skinList) 
                         {
                             dynamicColors = skinList.colors;
                             Debug.Log($"[Controller] Cast to SkinColorList Success. Count: {(dynamicColors?.Count ?? 0)}");
                         }
                         
                         // Strategy 2: Reflection (Fallback)
                         if(dynamicColors == null)
                         {
                             Debug.LogWarning($"[Controller] Cast Failed. Trying Reflection on {targetPalette.name}...");
                             try
                             {
                                 FieldInfo field = targetPalette.GetType().GetField("colors");
                                 if(field != null)
                                 {
                                     dynamicColors = field.GetValue(targetPalette) as List<Color>;
                                     Debug.Log($"[Controller] Reflection Success. Count: {(dynamicColors?.Count ?? 0)}");
                                 }
                                 else Debug.LogError($"[Controller] Reflection Failed: 'colors' field not found on {targetPalette.GetType().Name}");
                             }
                             catch(System.Exception e) { Debug.LogError($"[Controller] Reflection Error for {targetPalette.name}: {e.Message}"); }
                         }
                     }
                     else 
                     {
                         Debug.LogWarning($"[Controller] Target Palette is NULL for {category}/{sub.subCategoryName}");
                     }
                     
                     // 2. Use Dynamic Colors if found
                     if(dynamicColors != null && dynamicColors.Count > 0)
                     {
                         actions.Add(() => LoadColorGrid(dynamicColors, (cIdx) => 
                         {
                             // Generic Color Apply Logic
                             currentBaseColor = dynamicColors[cIdx];
                             OnToneSliderChanged(0.5f);
                             if(uiManager.colorPalettePanel != null) uiManager.colorPalettePanel.GetComponentInChildren<Slider>().value = 0.5f;
                             uiManager.UpdateSliderVisual(uiManager.colorPalettePanel.GetComponentInChildren<Slider>(), currentBaseColor, modifier);
                         }));
                     }
                     else
                     {
                         // 3. Fallback to Global Lists OR Clear
                         actions.Add(() => {
                             Debug.LogWarning($"[Controller] ColorPalette action invoked but no colors found for {category}. Clearing grid.");
                             // Ensure grid is cleared if we fail to find colors
                             uiManager.ClearItemsGrid(); 
                             LoadColorsForCategory(category);
                         });
                     }
                 }
                 else if(sub.type == SubCategoryType.Folder)
                 {
                     actions.Add(() => LoadItemsForCategory(category, sub.path, catTab)); // Path e.g. "BoyHair"
                 }
                 else if(sub.type == SubCategoryType.DirectItems)
                 {
                     actions.Add(() => LoadItemsForCategory(category, sub.path ?? "", catTab));
                 }
             }
             
             uiManager.PopulateSubCategories(icons, (idx) => actions[idx].Invoke());
             
             // Auto Select First Option if requested
             if(catTab.autoSelectFirst && actions.Count > 0)
             {
                 int bestIndex = 0;
                 for (int i = 0; i < catTab.subCategories.Count; i++)
                 {
                     if (catTab.subCategories[i].type == SubCategoryType.Folder || 
                         catTab.subCategories[i].type == SubCategoryType.DirectItems)
                     {
                         bestIndex = i;
                         break;
                     }
                 }
                 
                 Debug.Log($"[Controller] Auto-selecting {category} sub-category index: {bestIndex} (Type: {catTab.subCategories[bestIndex].type})");
                 actions[bestIndex].Invoke();
             }
             return;
        }

        // 3. Fallback to Hardcoded/Auto Logic (if no component attached/configured)
        
        // ... (Keep existing auto-logic for backward compat) ...
        
        // For brevity in this edit, I will simplify the "Auto Logic" to what we had, 
        // effectively making "Skin" and others work even if they don't have the script yet.
        
        if (category == "Skin")
        {
             // Skin uses Special logic for now (Direct Color Grid)
             // Ideally Skin should also be a CategoryTab with type ColorPalette
              LoadColorGrid(skinColors, (colorIndex) => 
             {
                 currentBaseColor = skinColors[colorIndex];
                 OnToneSliderChanged(0.5f); 
                 if(uiManager.colorPalettePanel != null) uiManager.colorPalettePanel.GetComponentInChildren<Slider>().value = 0.5f;
                 
                 uiManager.UpdateSliderVisual(uiManager.colorPalettePanel.GetComponentInChildren<Slider>(), currentBaseColor, modifier);
             });
        }
        else
        {
             // Direct Load Default
             LoadItemsForCategory(category, "", catTab);
        }
    }
    
    private CategoryTab FindCategoryTab(string category)
    {
        // 1. Look in UI Manager's tab parent
        if(uiManager.categoryTabParent != null)
        {
            foreach(Transform child in uiManager.categoryTabParent)
            {
                CategoryTab tab = child.GetComponent<CategoryTab>();
                if(tab != null && tab.categoryId == category) return tab;
                if(tab != null && child.name.Contains(category)) return tab; // Fallback match
            }
        }
        return null;
    }

    private void LoadColorGrid(List<Color> colors, System.Action<int> onColorClick)
    {
        uiManager.SetColorPaletteActive(true); // 🔥 Show Slider when loading colors
        uiManager.PopulateColorGrid(colors, onColorClick);
    }

    private void LoadItemsForCategory(string category, string style, CategoryTab tab = null)
    {
        uiManager.SetColorPaletteActive(false);
        
        string path = GetCategoryPath(category, style, tab);
        Debug.Log($"[Controller] LoadItemsForCategory: category={category}, style={style} -> FINAL PATH: {path}");
        
        Sprite[] sprites = Resources.LoadAll<Sprite>(path);
        Debug.Log($"[Controller] Resources.LoadAll found {sprites.Length} sprites at {path}");
        
        if (sprites == null || sprites.Length == 0) 
        {
            Debug.LogWarning($"[Controller] No sprites found at {path}");
            return;
        }
        
        List<Sprite> spriteList = new List<Sprite>(sprites);

        uiManager.PopulateGrid(spriteList, (itemIndex) => 
        {
            string partName = GetPartNameFromCategory(category); 
            modifier.SetBodyPartSprite(currentCharacter, partName, spriteList[itemIndex]);
            
            // 🎯 v13: Exact path for JSON
            modifier.RegisterPartPath(partName, path);
        });
    }

    private void LoadColorsForCategory(string category)
    {
         List<Color> targetColors = null;
         if(category.Contains("Hair") || category == "Beard") targetColors = hairColors;
         else if(category == "Eyes") targetColors = eyesColors;
         else if(category == "Skin") targetColors = skinColors;
         else if(category == "Eyebrows") targetColors = hairColors; // Use hair colors for eyebrows
         
         if(targetColors != null)
         {
             LoadColorGrid(targetColors, (cIdx) => 
             {
                 currentBaseColor = targetColors[cIdx];
                 OnToneSliderChanged(0.5f);
                 if(uiManager.colorPalettePanel != null) uiManager.colorPalettePanel.GetComponentInChildren<Slider>().value = 0.5f;
                 uiManager.UpdateSliderVisual(uiManager.colorPalettePanel.GetComponentInChildren<Slider>(), currentBaseColor, modifier);
             });
         }
    }
    
    // Helper to centralize path logic
    private string GetCategoryPath(string category, string style, CategoryTab tab = null)
    {
        string basePath = "";
        
        // 0. Priority: Manual basePath from Component
        if (tab != null && !string.IsNullOrEmpty(tab.basePath))
        {
            basePath = tab.basePath;
        }
        else
        {
            // 1. Get Hardcoded Base Mapping
            if(category == "BoyHair") basePath = "Images/Character/Style/Hair_Image/BoyHair";
            else if(category == "GirlHair") basePath = "Images/Character/Style/Hair_Image/GirlHair";
            else if(category == "MixedHair") basePath = "Images/Character/Style/Hair_Image/MixedHair";
            else if(category == "Beard") basePath = "Images/Character/Style/Beard_Image";
            else if(category == "Eyes") basePath = "Images/Character/Style/Eyes_Image";
            else if(category == "Nose") basePath = "Images/Character/Style/Noise_Image"; 
            else if(category == "Eyebrows") basePath = "Images/Character/Style/EyeBrown_Image";
            else if(category == "Freckles") basePath = "Images/Character/Style/Freckle_Image";
            else if(category == "Mouth") basePath = "Images/Character/Style/Mouth_Image";
            else if(category == "Clothes") basePath = "Images/Character/Style/Clothes_Image";
            else if(category == "Hats") basePath = "Images/Character/Style/Hats_Image";
            else if(category == "Accessory") basePath = "Images/Character/Style/Accessory_Image";
            else basePath = $"Images/Character/Style/{category}";
        }
        // 2. Handle Style/SubPath
        if (string.IsNullOrEmpty(style)) return basePath;
        
        // If style is an absolute Resources path
        if (style.StartsWith("Images/")) return style;
        
        // Else append to base
        string finalPath = basePath;
        if (!string.IsNullOrEmpty(style))
        {
             // If style name is the same as the end of the base path, don't double it
             // e.g. basePath=".../BoyHair" and style="BoyHair" -> don't do ".../BoyHair/BoyHair"
             if (basePath.EndsWith("/" + style) || basePath.EndsWith("\\" + style))
             {
                 Debug.Log($"[Controller] Style '{style}' matches end of basePath. Using basePath as is.");
             }
             else
             {
                 finalPath = $"{basePath.TrimEnd('/', '\\')}/{style}";
             }
        }

        Debug.Log($"[Controller] GetCategoryPath Result: {finalPath}");
        return finalPath;
    }

    private string GetPartNameFromCategory(string cat)
    {
        if(cat.Contains("Hair")) return "Hair";
        if(cat == "Hats") return "Hat"; 
        if(cat == "Eyebrows") return "EyeBrown"; 
        if(cat == "Nose") return "Noise";
        if(cat == "Glasses") return "Accessory";
        
        return cat;
    }
}
