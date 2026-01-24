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
        if (currentCharacter != null && currentCharacter != character) 
        {
             // Eski referans varsa temizle (gerekirse)
        }
        
        currentCharacter = character;
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

                     // 1. Try Dragged ScriptableObject (SubCategory FIRST, then Main Tab)
                     Object targetPalette = sub.paletteObject != null ? sub.paletteObject : catTab.paletteObject;
                     
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
                     actions.Add(() => LoadItemsForCategory(category, sub.path)); // Path e.g. "BoyHair"
                 }
                 else if(sub.type == SubCategoryType.DirectItems)
                 {
                     actions.Add(() => LoadItemsForCategory(category, sub.path ?? ""));
                 }
             }
             
             uiManager.PopulateSubCategories(icons, (idx) => actions[idx].Invoke());
             
             // Auto Select First Option if requested (e.g. Skin -> Color Palette)
             if(catTab.autoSelectFirst && actions.Count > 0)
             {
                 actions[0].Invoke();
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
            uiManager.SetColorPaletteActive(true);
        }
        else
        {
             // Direct Load Default
             LoadItemsForCategory(category, "");
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
        uiManager.PopulateColorGrid(colors, onColorClick);
    }

    private void LoadItemsForCategory(string category, string style)
    {
        uiManager.SetColorPaletteActive(false);
        
        string path;
        // If style (sub-path) is provided via Component, use it directly or combine
        if(!string.IsNullOrEmpty(style))
        {
             // If style is a full path (e.g. contains "Images/"), use it. Else append.
             if(style.Contains("/")) path = style;
             else path = $"Images/Character/Style/{category}/{style}";
        }
        else
        {
             path = GetCategoryPath(category, "");
        }

        Sprite[] sprites = Resources.LoadAll<Sprite>(path);
        
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
        });
    }

    private void LoadColorsForCategory(string category)
    {
         uiManager.SetColorPaletteActive(true);
         
         List<Color> targetColors = null;
         if(category.Contains("Hair") || category == "Beard" || category == "EyeBrown") targetColors = hairColors;
         else if(category == "Eyes") targetColors = eyesColors;
         else if(category == "Skin") targetColors = skinColors;
         
         if(targetColors != null)
         {
             uiManager.PopulateColorGrid(targetColors, (cIdx) => 
             {
                 currentBaseColor = targetColors[cIdx];
                 OnToneSliderChanged(0.5f);
                 if(uiManager.colorPalettePanel != null) uiManager.colorPalettePanel.GetComponentInChildren<Slider>().value = 0.5f;
                 uiManager.UpdateSliderVisual(uiManager.colorPalettePanel.GetComponentInChildren<Slider>(), currentBaseColor, modifier);
             });
         }
    }
    
    // Helper to centralize path logic
    private string GetCategoryPath(string category, string style)
    {
        if(!string.IsNullOrEmpty(style))
        {
             return $"Images/Character/Style/{category}/{style}";
        }
        
        // Mapped Paths
        if(category == "BoyHair") return "Images/Character/Style/Hair_Image/BoyHair";
        if(category == "GirlHair") return "Images/Character/Style/Hair_Image/GirlHair";
        if(category == "MixedHair") return "Images/Character/Style/Hair_Image/MixedHair";
        if(category == "Beard") return "Images/Character/Style/Beard_Image";
        if(category == "Eyes") return "Images/Character/Style/Eyes_Image";
        if(category == "Nose") return "Images/Character/Style/Noise_Image"; 
        if(category == "Eyebrows") return "Images/Character/Style/EyeBrown_Image";
        if(category == "Freckles") return "Images/Character/Style/Freckle_Image";
        if(category == "Mouth") return "Images/Character/Style/Mouth_Image";
        if(category == "Clothes") return "Images/Character/Style/Clothes_Image";
        if(category == "Hats") return "Images/Character/Style/Hats_Image";
        if(category == "Accessory") return "Images/Character/Style/Accessory_Image";
        
        return $"Images/Character/Style/{category}"; 
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
