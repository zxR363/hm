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

    [Header("Referanslar")]
    public GameObject optionItemPrefab;
    public Transform optionGridParent;

    [Header("Customization Options")]

    public List<Sprite> skinColorIcons; // Her renk için bir ikon (örneğin renkli daireler)
    public List<Color> skinColors;      // Gerçek renk değerleri (karaktere uygulanacak)

    public List<Sprite> hairBoy_Sprites;
    public List<Sprite> hairGirl_Sprites;
    public List<Sprite> hairMixed_Sprites;

    public List<Sprite> outfitSprites;
    public List<Sprite> accessorySprites;



    public EnumCharacterCustomizationCategory currentCategory;


    void Start()
    {
        skinColorIcons = LoadSpritesFromResources("Images/Character/Style/Skin");
        skinColors = LoadSkinColors(); // Aşağıda açıklanıyor

        hairBoy_Sprites = LoadSpritesFromResources("Images/Character/Style/Hair/BoyHair");
        hairGirl_Sprites = LoadSpritesFromResources("Images/Character/Style/Hair/GirlHair");
        hairMixed_Sprites = LoadSpritesFromResources("Images/Character/Style/Hair/MixedHair");
        outfitSprites = LoadSpritesFromResources("Images/Character/Style/Outfit");
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

    public void SelectHair(int index,string style)
    {

        if (previewInstance == null) return;

        var hairImage = previewInstance.transform.Find("Hair").GetComponent<Image>();

        switch(style)
        {
            case "boy":
                hairImage.sprite = hairBoy_Sprites[index];
                break;
            case "girl":
                hairImage.sprite = hairGirl_Sprites[index];
                break;
            case "mixed":
                hairImage.sprite = hairMixed_Sprites[index];
                break;
            default:
                Debug.Log("Select Hair ERROR!!!!");
                break;
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
    }

    public void SelectOutfit(int index)
    {
        if (previewInstance == null) return;

        var outfitImage = previewInstance.transform.Find("Outfit").GetComponent<Image>();
        outfitImage.sprite = outfitSprites[index];
    }

    public void SelectAccessory(int index)
    {
        if (previewInstance == null) return;

        var accessoryImage = previewInstance.transform.Find("Accessory").GetComponent<Image>();
        accessoryImage.sprite = accessorySprites[index];
    }

    //--------------PREVIEW AREA-------------------

    //--------------SELECTION TAB And OptionGRID-------------------

    public void SetCategory(int currentCategoryR)
    {
        EnumCharacterCustomizationCategory tmpCurrentCategory = (EnumCharacterCustomizationCategory) currentCategoryR;
        currentCategory = tmpCurrentCategory;
        switch (tmpCurrentCategory)
        {
            case EnumCharacterCustomizationCategory.Skin:
                Populate_Skin_Options();
                break;
            case EnumCharacterCustomizationCategory.Hair_Boy:
                Populate_HairBoy_Options();
                break;
            case EnumCharacterCustomizationCategory.Hair_Girl:
                Populate_HairGirl_Options();
                break;
            case EnumCharacterCustomizationCategory.Hair_Mixed:
                Populate_HairMixed_Options();
                break;
            case EnumCharacterCustomizationCategory.Outfit:
                Populate_Outfit_Options();
                break;
            case EnumCharacterCustomizationCategory.Accessories:
                Populate_Accessory_Options();
                break;
        }
        Debug.Log("currentCategory="+currentCategory);
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


    public void Populate_Outfit_Options()
    {
        ClearOptionGrid();

        for (int i = 0; i < outfitSprites.Count; i++)
        {
            GameObject item = Instantiate(optionItemPrefab, optionGridParent);
            OptionItem option = item.GetComponent<OptionItem>();
            option.Setup(outfitSprites[i], i, this);

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


    private void ClearOptionGrid()
    {
        foreach (Transform child in optionGridParent)
            Destroy(child.gameObject);
    }


    //--------------SELECTION TAB And OptionGRID-------------------


    //-------
    private List<Sprite> LoadSpritesFromResources(string path)
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
