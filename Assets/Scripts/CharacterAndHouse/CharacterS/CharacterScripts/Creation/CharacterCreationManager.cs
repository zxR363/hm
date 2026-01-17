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
        ClearOptionGrid();
        string fullPath = $"Images/Character/Style/{pathSuffix}";
        List<Sprite> sprites = GetOrLoadSprites(fullPath);
        Debug.Log($"[Optimization] Loaded {sprites.Count} sprites from {fullPath}");

        for (int i = 0; i < sprites.Count; i++)
        {
            GameObject item = Instantiate(optionItemPrefab, optionGridParent);
            OptionItem option = item.GetComponent<OptionItem>();
            option.Setup(sprites[i], i, this, styleKey);
            item.SetActive(true);
            item.GetComponent<Button>().onClick.AddListener(option.OnClick);
        }
    }

    public void SelectPartGeneric(string partName, string resourceFolder, int index, string style = "")
    {
        if (previewInstance == null) return;
        
        Transform partT = FindPart(previewInstance.transform, partName);
        if (partT == null && partName == "Hat") partT = FindPart(previewInstance.transform, "Hats");
        if (partT == null) return;

        Image partImage = partT.GetComponent<Image>();
        if (partImage == null) return;

        string path = $"Images/Character/Style/{resourceFolder}";
        if (!string.IsNullOrEmpty(style)) path += $"/{style}";

        List<Sprite> sprites = GetOrLoadSprites(path);
        if (index >= 0 && index < sprites.Count)
        {
            partImage.sprite = sprites[index];
            ImageSettingsApplier applier = partT.GetComponent<ImageSettingsApplier>();
            if (applier != null) applier.ApplySettings();
        }
    }


    void Start()
    {
        // 🎯 OPTIMIZATION: Do NOT load everything at start. Resources are loaded on-demand now.
        skinColorIcons = LoadSpritesFromResources("Images/Character/Style/Skin_Image");
        skinColors = LoadSkinColors(); 
        
        LoadItemColors(); // Small enough to keep

        SpawnPreviewCharacter();
        SetCategory(0); // Default to Skin
    }

    //--------------PREVIEW AREA-------------------
    void SpawnPreviewCharacter()
    {
        if (previewInstance != null) Destroy(previewInstance);
        
        //Eğer dinamik characterPreFabPreview Istersem kullanırım
        //previewInstance = Instantiate(characterPrefab, previewArea);
        //previewInstance.transform.localPosition = characterPrefab.transform.localPosition;
        //previewInstance.transform.localScale = characterPrefab.transform.localScale;
        //previewInstance.SetActive(true);
        previewInstance = characterPrefab;

        //Eğer dinamik characterPreFabPreview Istersem kullanırım
        //previewInstance = Instantiate(characterPrefab, previewArea);
        //previewInstance.transform.localPosition = characterPrefab.transform.localPosition;
        //previewInstance.transform.localScale = characterPrefab.transform.localScale;
        //previewInstance.SetActive(true);
        previewInstance = characterPrefab;
    }

    //Secilen renk ilgili GameObject'in rengini güncelliyor(Skin haric. Örn:Hair,EyeBrown)
    public void SelectColorPalette(int index)
    {
        // Debug.Log("ColorPalette1111 = "+colorRoot + "  "+index);
        // Debug.Log("AAAAAA="+colorValue.Count);

        if (previewInstance == null) return;

        if (colorRoot == null) return;

        Color selectedColor = colorValue[index];
        selectedColor.a = 1f; // Şeffaflık önlemi

        Image rootImage = colorRoot.GetComponent<Image>();

        if (rootImage != null)
            rootImage.color = selectedColor;

        // Tüm child'lara uygula
        foreach (Transform child in colorRoot)
        {
            Image childImage = child.GetComponent<Image>();
            if (childImage != null)
                childImage.color = selectedColor;
        }
    }

    public void SelectSkinColor(int index)
    {
        if (previewInstance == null) return;

        // Recursive arama yapacak helper fonksiyon
        Transform skinRoot = FindPart(previewInstance.transform, "Skin");
        
        if (skinRoot == null) return;

        Color selectedColor = skinColors[index];
        selectedColor.a = 1f; // Şeffaflık önlemi

        Image rootImage = skinRoot.GetComponent<Image>();
        //if (rootImage != null)
        //    rootImage.color = selectedColor;

        // Tüm child'lara uygula
        foreach (Transform child in skinRoot)
        {
            Image childImage = child.GetComponent<Image>();
            if (childImage != null)
                childImage.color = selectedColor;
        }

        //ToneSliderArea aktif hale getirildi ve renk degişecek gameObject' iletildi.
        dynamicCategoryManager.setActiveCategorySelectedToneSliderArea(true,skinRoot,true); 
    }

        // --- REDIRECTS TO GENERIC ---
    public void SelectHair(int index, string style) => SelectPartGeneric("Hair", "Hair_Image", index, style);
    public void SelectClothes(int index, string style) => SelectPartGeneric("Clothes", "Clothes_Image", index, style);
    public void SelectHat(int index, string style) => SelectPartGeneric("Hat", "Hats_Image", index, style);
    public void SelectAccessory(int index, string style) => SelectPartGeneric("Accessory", "Accessory_Image", index, style);
    public void SelectMouth(int index, string style) => SelectPartGeneric("Mouth", "Mouth_Image", index, style);
    
    // Original methods for Beard, Eyes, Noise etc. were not implemented in the snippet provided.
    // If they were adding logic to SelectPartGeneric above, we would map them too.
    // For now we assume these are the main ones present in the file chunk we saw.
    public void SelectBeard(int index, string style) => SelectPartGeneric("Beard", "Outfit", index, style);
    public void SelectEyes(int index, string style) => SelectPartGeneric("Eyes", "Outfit", index, style);
    public void SelectNoise(int index, string style) => SelectPartGeneric("Noise", "Outfit", index, style);
    public void SelectEyeBrown(int index, string style) => SelectPartGeneric("EyeBrown", "Outfit", index, style);
    public void SelectFreckle(int index, string style) => SelectPartGeneric("Freckle", "Outfit", index, style);

    //--------------PREVIEW AREA-------------------

    //------------------------------------------------------------------------------------
    //-------***********************************************************-------------------
    //--------------SELECTION TAB And OptionGRID-------------------

    public void SetCategory(int currentCategoryR)
    {
        EnumCharacterCustomizationCategory tmpCurrentCategory = (EnumCharacterCustomizationCategory) currentCategoryR;
        currentCategory = tmpCurrentCategory;

        // 🔥 Her kategori değişiminde eski butonları temizle
        dynamicCategoryManager.ClearGrid(dynamicCategoryManager.categoryGridParent);


        switch (tmpCurrentCategory)
        {
            case EnumCharacterCustomizationCategory.Skin: 
                Populate_Skin_Options();
                break;
            
            //Direkt Buton ile açılanlar
            case EnumCharacterCustomizationCategory.Hair_Boy:
                colorRoot = FindPart(previewInstance.transform, "Hair");
                //ToneSliderArea aktif hale getirildi ve renk degişecek gameObject' iletildi.
                dynamicCategoryManager.setActiveCategorySelectedToneSliderArea(true,colorRoot,false); 
                dynamicCategoryManager.PopulateOptionColorPalette();

                dynamicCategoryManager.PopulateOptionGrid("Hair_Image","BoyHair");
                break;

            case EnumCharacterCustomizationCategory.Hair_Girl:
                colorRoot = FindPart(previewInstance.transform, "Hair");
                //ToneSliderArea aktif hale getirildi ve renk degişecek gameObject' iletildi.
                dynamicCategoryManager.setActiveCategorySelectedToneSliderArea(true,colorRoot,false); 
                dynamicCategoryManager.PopulateOptionColorPalette();

                dynamicCategoryManager.PopulateOptionGrid("Hair_Image", "GirlHair");
                break;

            case EnumCharacterCustomizationCategory.Hair_Mixed:
                colorRoot = FindPart(previewInstance.transform, "Hair");
                //ToneSliderArea aktif hale getirildi ve renk degişecek gameObject' iletildi.
                dynamicCategoryManager.setActiveCategorySelectedToneSliderArea(true,colorRoot,false); 
                dynamicCategoryManager.PopulateOptionColorPalette();

                dynamicCategoryManager.PopulateOptionGrid("Hair_Image", "MixedHair");
                break;
            
            case EnumCharacterCustomizationCategory.Beard:
                colorRoot = FindPart(previewInstance.transform, "Beard");
                //ToneSliderArea aktif hale getirildi ve renk degişecek gameObject' iletildi.
                dynamicCategoryManager.setActiveCategorySelectedToneSliderArea(true,colorRoot,false); 
                dynamicCategoryManager.PopulateOptionColorPalette();

                dynamicCategoryManager.PopulateOptionGrid("Beard_Image", "");
                break;
            case EnumCharacterCustomizationCategory.Eyes:
                colorRoot = FindPart(previewInstance.transform, "Eyes");
                 //ToneSliderArea aktif hale getirildi ve renk degişecek gameObject' iletildi.
                dynamicCategoryManager.setActiveCategorySelectedToneSliderArea(true,colorRoot,false); 
                dynamicCategoryManager.PopulateOptionColorPalette();

                dynamicCategoryManager.PopulateOptionGrid("Eyes_Image", "");
                break;
            case EnumCharacterCustomizationCategory.Noise:
                colorRoot = FindPart(previewInstance.transform, "Noise");
                //ToneSliderArea aktif hale getirildi ve renk degişecek gameObject' iletildi.
                dynamicCategoryManager.setActiveCategorySelectedToneSliderArea(true,colorRoot,false); 
                dynamicCategoryManager.PopulateOptionColorPalette();

                dynamicCategoryManager.PopulateOptionGrid("Noise_Image", "");
                break;    
            case EnumCharacterCustomizationCategory.EyeBrown:
                colorRoot = FindPart(previewInstance.transform, "EyeBrown");
                //ToneSliderArea aktif hale getirildi ve renk degişecek gameObject' iletildi.
                dynamicCategoryManager.setActiveCategorySelectedToneSliderArea(true,colorRoot,false); 
                dynamicCategoryManager.PopulateOptionColorPalette();

                dynamicCategoryManager.PopulateOptionGrid("EyeBrown_Image", "");
                break;
            case EnumCharacterCustomizationCategory.Freckle:
                colorRoot = FindPart(previewInstance.transform, "Freckle");
                //ToneSliderArea aktif hale getirildi ve renk degişecek gameObject' iletildi.
                dynamicCategoryManager.setActiveCategorySelectedToneSliderArea(true,colorRoot,false); 
                dynamicCategoryManager.PopulateOptionColorPalette();

                dynamicCategoryManager.PopulateOptionGrid("Freckle_Image", "");
                break;                                                            
            
            //Alt seçim yapılarak açılanlar
            case EnumCharacterCustomizationCategory.Clothes:
                colorRoot = FindPart(previewInstance.transform, "Clothes");
                //ToneSliderArea aktif hale getirildi ve renk degişecek gameObject' iletildi.
                dynamicCategoryManager.setActiveCategorySelectedToneSliderArea(false,colorRoot,false); 

                dynamicCategoryManager.PopulateCategoryButtons("Clothes_Image");
                break;

            case EnumCharacterCustomizationCategory.Hats:
                colorRoot = FindPart(previewInstance.transform, "Hat"); // Duzeltildi: "Hats" degil "Hat" olabilir, ama rig aracinda "Hat" kullandik. Kontrol gerekirse isme gore.
                if(colorRoot == null) colorRoot = FindPart(previewInstance.transform, "Hats");
                //ToneSliderArea aktif hale getirildi ve renk degişecek gameObject' iletildi.
                dynamicCategoryManager.setActiveCategorySelectedToneSliderArea(false,colorRoot,false); 

                dynamicCategoryManager.PopulateCategoryButtons("Hats_Image");
                break;

            case EnumCharacterCustomizationCategory.Accessory:
                colorRoot = FindPart(previewInstance.transform, "Accessory");
                //ToneSliderArea aktif hale getirildi ve renk degişecek gameObject' iletildi.
                dynamicCategoryManager.setActiveCategorySelectedToneSliderArea(false,colorRoot,false);

                dynamicCategoryManager.PopulateCategoryButtons("Accessory_Image");
                break;

            case EnumCharacterCustomizationCategory.Mouth:
                colorRoot = FindPart(previewInstance.transform, "Mouth");
                //ToneSliderArea aktif hale getirildi ve renk degişecek gameObject' iletildi.
                dynamicCategoryManager.setActiveCategorySelectedToneSliderArea(false,colorRoot,false); 
                dynamicCategoryManager.PopulateOptionColorPalette();

                dynamicCategoryManager.PopulateOptionGrid("Mouth_Image", "");
                break;

            // Diğer kategoriler eklenebilir
        }
        
    }

    //Seçilen Renklerin uygulanabilmesi için yapılıyor.
    public void Populate_ColorPalette_Options()
    {
        ClearOptionGrid();
        switch (currentCategory)
        {
            case EnumCharacterCustomizationCategory.Hair_Boy:
            case EnumCharacterCustomizationCategory.Hair_Girl:
            case EnumCharacterCustomizationCategory.Hair_Mixed:
                colorValue = hairColors;
                colorIcons = hairColorIcons;
                colorRoot = FindPart(previewInstance.transform, "Hair");
                break;
            
            case EnumCharacterCustomizationCategory.Beard:
                break;
            case EnumCharacterCustomizationCategory.Eyes:
                break;
            case EnumCharacterCustomizationCategory.Noise:
                colorValue = noiseColors;
                colorIcons = noiseColorIcons;
                colorRoot = FindPart(previewInstance.transform, "Noise");
                break;    
            case EnumCharacterCustomizationCategory.EyeBrown:
                colorValue = eyeBrownColors;
                colorIcons = eyeBrownColorIcons;
                colorRoot = FindPart(previewInstance.transform, "EyeBrown");
                break;
            case EnumCharacterCustomizationCategory.Freckle:
                colorValue = freckleColors;
                colorIcons = freckleColorIcons;
                colorRoot = FindPart(previewInstance.transform, "Freckle");
                break;                                                            
        }

        //Debug.Log("PALETTE TIKLANDI="+colorRoot+ "  "+colorValue.Count);

        for (int i = 0; i < colorValue.Count; i++)
        {
            GameObject item = Instantiate(optionItemPrefab, optionGridParent);
            OptionItem option = item.GetComponent<OptionItem>();

            Sprite icon = colorIcons[i]; // Hazır ikon kullan
            
            option.Setup(icon, i, this,null,1);

            item.SetActive(true);
            item.GetComponent<Button>().onClick.AddListener(option.OnClick);
        }

    }

    public void Populate_Skin_Options()
    {
        ClearOptionGrid();

        for (int i = 0; i < skinColors.Count; i++)
        {
            GameObject item = Instantiate(optionItemPrefab, optionGridParent);
            OptionItem option = item.GetComponent<OptionItem>();

            Sprite icon = skinColorIcons[i]; // Hazır ikon kullan
            
            option.Setup(icon, i, this,null,0);

            item.SetActive(true);
            item.GetComponent<Button>().onClick.AddListener(option.OnClick);
        }
  
        //ToneSliderArea aktif hale getirildi ve renk degişecek gameObject' iletildi.
        Transform skinRoot = FindPart(previewInstance.transform, "Skin");
        dynamicCategoryManager.setActiveCategorySelectedToneSliderArea(true,skinRoot,true); 

    }

    public void Populate_HairBoy_Options() => PopulateOptionsGeneric("Hair_Image/BoyHair", "BoyHair");
    public void Populate_HairGirl_Options() => PopulateOptionsGeneric("Hair_Image/GirlHair", "GirlHair");
    public void Populate_HairMixed_Options() => PopulateOptionsGeneric("Hair_Image/MixedHair", "MixedHair");
    public void Populate_Accessory_Options() => PopulateOptionsGeneric("Accessory_Image/Accessories", "Accessories"); // Or "Accessory" check folder name
    // Original was: accessorySprites = ...(".../Accessories");
    // SelectAccessory generic redirect does: SelectPartGeneric("Accessory", "Accessory_Image", index, style);
    // Path constructed: "Accessory_Image/{style}"
    // So if style is "Accessories", path is "Accessory_Image/Accessories". Correct.

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
        foreach (Transform child in optionGridParent)
            Destroy(child.gameObject);
    }

    //-------
    public List<Sprite> LoadSpritesFromResources(string path)
    {
        Sprite[] loaded = Resources.LoadAll<Sprite>(path);
        return new List<Sprite>(loaded);
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
