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

    public EnumCharacterCustomizationCategory currentCategory;

    //--------------Item Color
    public List<Sprite> colorIcons;
    public List<Color> colorValue;
    public Transform colorRoot;

    public List<Sprite> hairColorIcons; // Her renk için bir ikon (örneğin renkli daireler)
    public List<Color> hairColors;      // Gerçek renk değerleri (karaktere uygulanacak)

    public List<Sprite> beardColorIcons; // Her renk için bir ikon (örneğin renkli daireler)
    public List<Color> beardColors;      // Gerçek renk değerleri (karaktere uygulanacak)

    public List<Sprite> eyesColorIcons; // Her renk için bir ikon (örneğin renkli daireler)
    public List<Color> eyesColors;      // Gerçek renk değerleri (karaktere uygulanacak)

    public List<Sprite> noiseColorIcons; // Her renk için bir ikon (örneğin renkli daireler)
    public List<Color> noiseColors;      // Gerçek renk değerleri (karaktere uygulanacak)

    public List<Sprite> eyeBrownColorIcons; // Her renk için bir ikon (örneğin renkli daireler)
    public List<Color> eyeBrownColors;      // Gerçek renk değerleri (karaktere uygulanacak)

    public List<Sprite> freckleColorIcons; // Her renk için bir ikon (örneğin renkli daireler)
    public List<Color> freckleColors;      // Gerçek renk değerleri (karaktere uygulanacak)
    //--------------Item Color


    void Start()
    {
        skinColorIcons = LoadSpritesFromResources("Images/Character/Style/Skin_Image");
        skinColors = LoadSkinColors(); // Aşağıda açıklanıyor

        LoadItemColors();

        hairBoy_Sprites = LoadSpritesFromResources("Images/Character/Style/Hair_Image/BoyHair");
        hairGirl_Sprites = LoadSpritesFromResources("Images/Character/Style/Hair_Image/GirlHair");
        hairMixed_Sprites = LoadSpritesFromResources("Images/Character/Style/Hair_Image/MixedHair");
        beardSprites = LoadSpritesFromResources("Images/Character/Style/Outfit");
        eyesSprites = LoadSpritesFromResources("Images/Character/Style/Outfit");
        noiseSprites = LoadSpritesFromResources("Images/Character/Style/Outfit");
        eyeBrownSprites = LoadSpritesFromResources("Images/Character/Style/Outfit");
        freckleSprites = LoadSpritesFromResources("Images/Character/Style/Outfit");
        clothesSprites = LoadSpritesFromResources("Images/Character/Style/Outfit");
        hatsSprites = LoadSpritesFromResources("Images/Character/Style/Outfit");
        accessorySprites = LoadSpritesFromResources("Images/Character/Style/Accessories");

        SpawnPreviewCharacter();
        SetCategory(0); // Varsayılan olarak "Skin" kategorisini seç
    }

    //--------------PREVIEW AREA-------------------
    void SpawnPreviewCharacter()
    {
        if (previewInstance != null)
            Destroy(previewInstance);

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

        Transform skinRoot = previewInstance.transform.Find("Skin");
        
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

        //Dinamik olarak seciyor
    public void SelectHair(int index, string style)
    {
        if (previewInstance == null) return;

        var hairImage = previewInstance.transform.Find("Hair").GetComponent<Image>();
        var sprites = LoadSpritesFromResources($"Images/Character/Style/Hair_Image/{style}");

        if (index >= 0 && index < sprites.Count)
            hairImage.sprite = sprites[index];
    }

    //Dinamik olarak clothes'u seçiyor. Diğer yapılarıda burdan referans alarak yapabiliriz(Örn. hats,accessory)
    public void SelectClothes(int index, string style)
    {
        if (previewInstance == null) return;
        
        var gameObj = previewInstance.transform.Find("Clothes");
        var clothesImage = gameObj.GetComponent<Image>();
        var sprites = LoadSpritesFromResources($"Images/Character/Style/Clothes_Image/{style}");

        if (index >= 0 && index < sprites.Count)
            clothesImage.sprite = sprites[index];

        // 🔥 Ek olarak ImageSettingsApplier varsa → ApplySettings() çağır
        ImageSettingsApplier applier = gameObj.GetComponent<ImageSettingsApplier>();
        if (applier != null)
            applier.ApplySettings();

    }

    public void SelectHat(int index, string style)
    {
        if (previewInstance == null) return;

        var gameObj = previewInstance.transform.Find("Hat");
        var hatImage = gameObj.GetComponent<Image>();
        var sprites = LoadSpritesFromResources($"Images/Character/Style/Hats_Image/{style}");

        if (index >= 0 && index < sprites.Count)
            hatImage.sprite = sprites[index];
        
        // 🔥 Ek olarak ImageSettingsApplier varsa → ApplySettings() çağır
        ImageSettingsApplier applier = gameObj.GetComponent<ImageSettingsApplier>();
        if (applier != null)
            applier.ApplySettings();
    }


    //Dinamik olarak seciyor
    public void SelectAccessory(int index, string style)
    {
        if (previewInstance == null) return;

        var gameObj = previewInstance.transform.Find("Accessory");
        var accessoryImage = gameObj.GetComponent<Image>();
        var sprites = LoadSpritesFromResources($"Images/Character/Style/Accessory_Image/{style}");

        if (index >= 0 && index < sprites.Count)
            accessoryImage.sprite = sprites[index];
        
        // 🔥 Ek olarak ImageSettingsApplier varsa → ApplySettings() çağır
        ImageSettingsApplier applier = gameObj.GetComponent<ImageSettingsApplier>();
        if (applier != null)
            applier.ApplySettings();
    }

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
                colorRoot = previewInstance.transform.Find("Hair");
                //ToneSliderArea aktif hale getirildi ve renk degişecek gameObject' iletildi.
                dynamicCategoryManager.setActiveCategorySelectedToneSliderArea(true,colorRoot,false); 
                dynamicCategoryManager.PopulateOptionColorPalette();

                dynamicCategoryManager.PopulateOptionGrid("Hair_Image","BoyHair");
                break;

            case EnumCharacterCustomizationCategory.Hair_Girl:
                colorRoot = previewInstance.transform.Find("Hair");
                //ToneSliderArea aktif hale getirildi ve renk degişecek gameObject' iletildi.
                dynamicCategoryManager.setActiveCategorySelectedToneSliderArea(true,colorRoot,false); 
                dynamicCategoryManager.PopulateOptionColorPalette();

                dynamicCategoryManager.PopulateOptionGrid("Hair_Image", "GirlHair");
                break;

            case EnumCharacterCustomizationCategory.Hair_Mixed:
                colorRoot = previewInstance.transform.Find("Hair");
                //ToneSliderArea aktif hale getirildi ve renk degişecek gameObject' iletildi.
                dynamicCategoryManager.setActiveCategorySelectedToneSliderArea(true,colorRoot,false); 
                dynamicCategoryManager.PopulateOptionColorPalette();

                dynamicCategoryManager.PopulateOptionGrid("Hair_Image", "MixedHair");
                break;
            
            case EnumCharacterCustomizationCategory.Beard:
                colorRoot = previewInstance.transform.Find("Beard");
                //ToneSliderArea aktif hale getirildi ve renk degişecek gameObject' iletildi.
                dynamicCategoryManager.setActiveCategorySelectedToneSliderArea(true,colorRoot,false); 
                dynamicCategoryManager.PopulateOptionColorPalette();

                dynamicCategoryManager.PopulateOptionGrid("Beard_Image", "");
                break;
            case EnumCharacterCustomizationCategory.Eyes:
                colorRoot = previewInstance.transform.Find("Eyes");
                 //ToneSliderArea aktif hale getirildi ve renk degişecek gameObject' iletildi.
                dynamicCategoryManager.setActiveCategorySelectedToneSliderArea(true,colorRoot,false); 
                dynamicCategoryManager.PopulateOptionColorPalette();

                dynamicCategoryManager.PopulateOptionGrid("Eyes_Image", "");
                break;
            case EnumCharacterCustomizationCategory.Noise:
                colorRoot = previewInstance.transform.Find("Noise");
                //ToneSliderArea aktif hale getirildi ve renk degişecek gameObject' iletildi.
                dynamicCategoryManager.setActiveCategorySelectedToneSliderArea(true,colorRoot,false); 
                dynamicCategoryManager.PopulateOptionColorPalette();

                dynamicCategoryManager.PopulateOptionGrid("Noise_Image", "");
                break;    
            case EnumCharacterCustomizationCategory.EyeBrown:
                colorRoot = previewInstance.transform.Find("EyeBrown");
                //ToneSliderArea aktif hale getirildi ve renk degişecek gameObject' iletildi.
                dynamicCategoryManager.setActiveCategorySelectedToneSliderArea(true,colorRoot,false); 
                dynamicCategoryManager.PopulateOptionColorPalette();

                dynamicCategoryManager.PopulateOptionGrid("EyeBrown_Image", "");
                break;
            case EnumCharacterCustomizationCategory.Freckle:
                colorRoot = previewInstance.transform.Find("Freckle");
                //ToneSliderArea aktif hale getirildi ve renk degişecek gameObject' iletildi.
                dynamicCategoryManager.setActiveCategorySelectedToneSliderArea(true,colorRoot,false); 
                dynamicCategoryManager.PopulateOptionColorPalette();

                dynamicCategoryManager.PopulateOptionGrid("Freckle_Image", "");
                break;                                                            
            
            //Alt seçim yapılarak açılanlar
            case EnumCharacterCustomizationCategory.Clothes:
                colorRoot = previewInstance.transform.Find("Clothes");
                //ToneSliderArea aktif hale getirildi ve renk degişecek gameObject' iletildi.
                dynamicCategoryManager.setActiveCategorySelectedToneSliderArea(false,colorRoot,false); 

                dynamicCategoryManager.PopulateCategoryButtons("Clothes_Image");
                break;

            case EnumCharacterCustomizationCategory.Hats:
                colorRoot = previewInstance.transform.Find("Hats");
                //ToneSliderArea aktif hale getirildi ve renk degişecek gameObject' iletildi.
                dynamicCategoryManager.setActiveCategorySelectedToneSliderArea(false,colorRoot,false); 

                dynamicCategoryManager.PopulateCategoryButtons("Hats_Image");
                break;

            case EnumCharacterCustomizationCategory.Accessory:
                colorRoot = previewInstance.transform.Find("Accessory");
                //ToneSliderArea aktif hale getirildi ve renk degişecek gameObject' iletildi.
                dynamicCategoryManager.setActiveCategorySelectedToneSliderArea(false,colorRoot,false);

                dynamicCategoryManager.PopulateCategoryButtons("Accessory_Image");
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
                colorRoot = previewInstance.transform.Find("Hair");
                break;
            
            case EnumCharacterCustomizationCategory.Beard:
                break;
            case EnumCharacterCustomizationCategory.Eyes:
                break;
            case EnumCharacterCustomizationCategory.Noise:
                colorValue = noiseColors;
                colorIcons = noiseColorIcons;
                colorRoot = previewInstance.transform.Find("Noise");
                break;    
            case EnumCharacterCustomizationCategory.EyeBrown:
                colorValue = eyeBrownColors;
                colorIcons = eyeBrownColorIcons;
                colorRoot = previewInstance.transform.Find("EyeBrown");
                break;
            case EnumCharacterCustomizationCategory.Freckle:
                colorValue = freckleColors;
                colorIcons = freckleColorIcons;
                colorRoot = previewInstance.transform.Find("Freckle");
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
        Transform skinRoot = previewInstance.transform.Find("Skin");
        dynamicCategoryManager.setActiveCategorySelectedToneSliderArea(true,skinRoot,true); 

    }

    public void Populate_HairBoy_Options()
    {
        ClearOptionGrid(); // Önce eski öğeleri sil

        for (int i = 0; i < hairBoy_Sprites.Count; i++)
        {
            GameObject item = Instantiate(optionItemPrefab, optionGridParent);
            OptionItem option = item.GetComponent<OptionItem>();
            option.Setup(hairBoy_Sprites[i], i, this,null,0);

            item.SetActive(true);
            item.GetComponent<Button>().onClick.AddListener(option.OnClick);
        }
    }

    public void Populate_HairGirl_Options()
    {
        ClearOptionGrid(); // Önce eski öğeleri sil

        for (int i = 0; i < hairGirl_Sprites.Count; i++)
        {
            GameObject item = Instantiate(optionItemPrefab, optionGridParent);
            OptionItem option = item.GetComponent<OptionItem>();
            option.Setup(hairGirl_Sprites[i], i, this,null,0);

            item.SetActive(true);
            item.GetComponent<Button>().onClick.AddListener(option.OnClick);
        }
    }

        public void Populate_HairMixed_Options()
    {
        ClearOptionGrid(); // Önce eski öğeleri sil

        for (int i = 0; i < hairMixed_Sprites.Count; i++)
        {
            GameObject item = Instantiate(optionItemPrefab, optionGridParent);
            OptionItem option = item.GetComponent<OptionItem>();
            option.Setup(hairMixed_Sprites[i], i, this);

            item.SetActive(true);
            item.GetComponent<Button>().onClick.AddListener(option.OnClick);
        }
    }


    public void Populate_Accessory_Options()
    {
        ClearOptionGrid();

        for (int i = 0; i < accessorySprites.Count; i++)
        {
            GameObject item = Instantiate(optionItemPrefab, optionGridParent);
            OptionItem option = item.GetComponent<OptionItem>();
            option.Setup(accessorySprites[i], i, this,null,0);

            item.SetActive(true);
            item.GetComponent<Button>().onClick.AddListener(option.OnClick);
        }
    }


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


}
