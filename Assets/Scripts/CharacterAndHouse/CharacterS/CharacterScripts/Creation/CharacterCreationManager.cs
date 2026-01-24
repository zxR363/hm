using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterCreationManager : MonoBehaviour
{
    [Header("Preview")]
    public Transform previewArea;
    public GameObject characterPrefab;
    public GameObject previewInstance; //Boş kalacak sonrasında otomatik doluyor

    [Header("Managers")]
    public DynamicCategoryManager dynamicCategoryManager;

    [Header("Referanslar")]
    public GameObject optionItemPrefab;
    public Transform optionGridParent;


    [Header("Customization Options")]

    public List<Sprite> skinColorIcons; // Her renk için bir ikon (örneğin renkli daireler)
    public List<Color> skinColors;      // Gerçek renk değerleri (karaktere uygulanacak)

    public List<Sprite> hairBoy_Sprites;
    public List<Sprite> hairGirl_Sprites;
    public List<Sprite> hairMixed_Sprites;

    public List<Sprite> beardSprites;
    public List<Sprite> eyesSprites;
    public List<Sprite> noiseSprites;
    public List<Sprite> eyeBrownSprites;
    public List<Sprite> freckleSprites;
    public List<Sprite> clothesSprites;
    public List<Sprite> hatsSprites;
    public List<Sprite> accessorySprites;
    public List<Sprite> mouthSprites;

    public EnumCharacterCustomizationCategory currentCategory;

    //--------------Item Color
    public List<Sprite> colorIcons;
    public List<Color> colorValue;
    public Transform colorRoot;

    // ... (Color Lists kept for now to avoid breaking existing logic) ...
    public List<Sprite> hairColorIcons; 
    public List<Color> hairColors;      
    public List<Sprite> beardColorIcons; 
    public List<Color> beardColors;      
    public List<Sprite> eyesColorIcons; 
    public List<Color> eyesColors;      
    public List<Sprite> noiseColorIcons; 
    public List<Color> noiseColors;      
    public List<Sprite> eyeBrownColorIcons; 
    public List<Color> eyeBrownColors;      
    public List<Sprite> freckleColorIcons; 
    public List<Color> freckleColors;      
    //--------------Item Color

    //-------------- OPTIMIZED RESOURCE MANAGEMENT -------------------
    private Dictionary<string, List<Sprite>> spriteCache = new Dictionary<string, List<Sprite>>();

    public List<Sprite> GetOrLoadSprites(string path)
    {
        if (spriteCache.ContainsKey(path)) return spriteCache[path];
        Sprite[] loaded = Resources.LoadAll<Sprite>(path);
        var list = new List<Sprite>(loaded);
        spriteCache[path] = list;
        return list;
    }

    public void PopulateOptionsGeneric(string pathSuffix, string styleKey = null)
    {
        // ❌ DEPRECATED: Logic moved to CharacterCreationController & UpgradeManager
    }

    public void SelectPartGeneric(string partName, string resourceFolder, int index, string style = "")
    {
        // ❌ DEPRECATED
    }

    void Start()
    {
        // 🎯 OPTIMIZATION: Do NOT load everything at start. Resources are loaded on-demand now.
        // skinColorIcons = LoadSpritesFromResources("Images/Character/Style/Skin_Image");
        // skinColors = LoadSkinColors(); 
        
        // LoadItemColors(); // Small enough to keep

        SpawnPreviewCharacter();
        // SetCategory(0); // ❌ REMOVED: Controller handles this now
    }

    //--------------PREVIEW AREA-------------------
    void SpawnPreviewCharacter()
    {
        if (previewInstance != null) Destroy(previewInstance);
        previewInstance = characterPrefab;
    }

    //Secilen renk ilgili GameObject'in rengini güncelliyor(Skin haric. Örn:Hair,EyeBrown)
    public void SelectColorPalette(int index)
    {
    }

    public void SelectSkinColor(int index)
    {
    }

        // --- REDIRECTS TO GENERIC ---
    public void SelectHair(int index, string style) { }
    public void SelectClothes(int index, string style) { }
    public void SelectHat(int index, string style) { }
    public void SelectAccessory(int index, string style) { }
    public void SelectMouth(int index, string style) { }
    
    public void SelectBeard(int index, string style) { }
    public void SelectEyes(int index, string style) { }
    public void SelectNoise(int index, string style) { }
    public void SelectEyeBrown(int index, string style) { }
    public void SelectFreckle(int index, string style) { }

    //--------------PREVIEW AREA-------------------

    //------------------------------------------------------------------------------------
    //-------***********************************************************-------------------
    //--------------SELECTION TAB And OptionGRID-------------------

    public void SetCategory(int currentCategoryR)
    {
        // ❌ DEPRECATED: Logic moved to CharacterCreationController
        Debug.LogWarning("[CharacterCreationManager] SetCategory is DEPRECATED and silenced.");
    }

    //Seçilen Renklerin uygulanabilmesi için yapılıyor.
    public void Populate_ColorPalette_Options()
    {
        // ❌ DEPRECATED
    }

    public void Populate_Skin_Options()
    {
        // ❌ DEPRECATED
    }

    // Generic redirects handled above.
    // Legacy functions removed.

    // Remove legacy unused lists if possible, but for now just redirect logic.
    // The Populate_Skin_Options uses specific logic (Skin Colors), keep it or genericize it?
    // It uses `skinColors` list, which is colors, not sprites. Keep logic for colors as is.

    // Generic redirects handled above.
    // Legacy functions removed.


    //------------------------------------------------------------------------------------
    //-------***********************************************************-------------------
    //--------------SELECTION TAB And OptionGRID-------------------

    private void ClearOptionGrid()
    {
    }

    //-------
    public List<Sprite> LoadSpritesFromResources(string path)
    {
        return new List<Sprite>();
    }

    private List<Color> LoadSkinColors()
    {
        var skinColorAsset = Resources.Load<SkinColorList>("Images/Character/Style/SkinColors/SkinColorList");
        return skinColorAsset != null ? skinColorAsset.colors : new List<Color>();
    }

    private void LoadItemColors()
    {
        hairColorIcons = LoadSpritesFromResources("Images/Character/Style/Items_Image/Item_Hair");
        var hairColorAsset = Resources.Load<ItemsColorList>("Images/Character/Style/ItemColors/HairColorList");
        hairColors = hairColorAsset != null ? hairColorAsset.colors : new List<Color>();

        beardColorIcons = LoadSpritesFromResources("Images/Character/Style/Items_Image/Item_Beard");
        var beardColorsAsset = Resources.Load<ItemsColorList>("Images/Character/Style/ItemColors/BeardColorList");
        beardColors = beardColorsAsset != null ? beardColorsAsset.colors : new List<Color>();

        eyesColorIcons = LoadSpritesFromResources("Images/Character/Style/Items_Image/Item_Eyes");
        var eyesColorsAsset = Resources.Load<ItemsColorList>("Images/Character/Style/ItemColors/EyesColorList");
        eyesColors = eyesColorsAsset != null ? eyesColorsAsset.colors : new List<Color>();

        noiseColorIcons = LoadSpritesFromResources("Images/Character/Style/Items_Image/Item_Noise");
        var noiseColorsAsset = Resources.Load<ItemsColorList>("Images/Character/Style/ItemColors/NoiseColorList");
        noiseColors = noiseColorsAsset != null ? noiseColorsAsset.colors : new List<Color>();

        eyeBrownColorIcons = LoadSpritesFromResources("Images/Character/Style/Items_Image/Item_EyeBrown");
        var eyeBrownColorsAsset = Resources.Load<ItemsColorList>("Images/Character/Style/ItemColors/EyeBrownColorList");
        eyeBrownColors = eyeBrownColorsAsset != null ? eyeBrownColorsAsset.colors : new List<Color>();

        freckleColorIcons = LoadSpritesFromResources("Images/Character/Style/Items_Image/Item_Freckle");
        var freckleColorsAsset = Resources.Load<ItemsColorList>("Images/Character/Style/ItemColors/FreckleColorList");
        freckleColors = freckleColorsAsset != null ? freckleColorsAsset.colors : new List<Color>();

        

        Debug.Log("hairColors="+hairColors.Count);
    }




    // --- YENI HELPER ---
    // Hiyerarsi degistigi icin Root altinda derinlemesine arama yapar
    private Transform FindPart(Transform root, string partName)
    {
        // 1. Dogrudan cocuk mu?
        Transform t = root.Find(partName);
        if (t != null) return t;

        // 2. Recursive (Derin) arama
        foreach (Transform child in root)
        {
            Transform result = FindPart(child, partName);
            if (result != null) return result;
        }

        return null;
    }
}
