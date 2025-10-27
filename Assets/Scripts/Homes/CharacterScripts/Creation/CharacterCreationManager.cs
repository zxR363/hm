using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterCreationManager : MonoBehaviour
{
    [Header("Preview")]
    public Transform previewArea;
    public GameObject characterPrefab;
    private GameObject previewInstance;

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


    void Start()
    {
        skinColorIcons = LoadSpritesFromResources("Images/Character/Style/Skin_Image");
        skinColors = LoadSkinColors(); // Aşağıda açıklanıyor

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
                dynamicCategoryManager.PopulateOptionGrid("Hair_Image","BoyHair");
                break;

            case EnumCharacterCustomizationCategory.Hair_Girl:
                dynamicCategoryManager.PopulateOptionGrid("Hair_Image", "GirlHair");
                break;

            case EnumCharacterCustomizationCategory.Hair_Mixed:
                dynamicCategoryManager.PopulateOptionGrid("Hair_Image", "MixedHair");
                break;
            
            case EnumCharacterCustomizationCategory.Beard:
                dynamicCategoryManager.PopulateOptionGrid("Beard_Image", "");
                break;
            case EnumCharacterCustomizationCategory.Eyes:
                dynamicCategoryManager.PopulateOptionGrid("Eyes_Image", "");
                break;
            case EnumCharacterCustomizationCategory.Noise:
                dynamicCategoryManager.PopulateOptionGrid("Noise_Image", "");
                break;    
            case EnumCharacterCustomizationCategory.EyeBrown:
                dynamicCategoryManager.PopulateOptionGrid("EyeBrown_Image", "");
                break;
            case EnumCharacterCustomizationCategory.Freckle:
                dynamicCategoryManager.PopulateOptionGrid("Freckle_Image", "");
                break;                                                            
            
            //Alt seçim yapılarak açılanlar
            case EnumCharacterCustomizationCategory.Clothes:
                dynamicCategoryManager.PopulateCategoryButtons("Clothes_Image");
                break;

            case EnumCharacterCustomizationCategory.Hats:
                dynamicCategoryManager.PopulateCategoryButtons("Hats_Image");
                break;

            case EnumCharacterCustomizationCategory.Accessories:
                dynamicCategoryManager.PopulateCategoryButtons("Accessory_Image");
                break;

            // Diğer kategoriler eklenebilir
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
            
            option.Setup(icon, i, this);

            item.SetActive(true);
            item.GetComponent<Button>().onClick.AddListener(option.OnClick);
        }
    }

    public void Populate_HairBoy_Options()
    {
        ClearOptionGrid(); // Önce eski öğeleri sil

        for (int i = 0; i < hairBoy_Sprites.Count; i++)
        {
            GameObject item = Instantiate(optionItemPrefab, optionGridParent);
            OptionItem option = item.GetComponent<OptionItem>();
            option.Setup(hairBoy_Sprites[i], i, this);

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
            option.Setup(hairGirl_Sprites[i], i, this);

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
            option.Setup(accessorySprites[i], i, this);

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


}
