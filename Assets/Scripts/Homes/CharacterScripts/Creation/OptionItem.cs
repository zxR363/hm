using UnityEngine;
using UnityEngine.UI;

public class OptionItem : MonoBehaviour
{
    public Image iconImage;

    private int optionIndex;
    private CharacterCreationManager manager;
    private EnumCharacterCustomizationCategory managerCategory;
    private string styleKey; // 🔑 Alt klasör adı (örneğin: "casual", "glasses", "crowns")

    private bool colorFlag = false;

    void Awake()
    {
        if (iconImage == null)
        {
            iconImage = GetComponentInChildren<Image>();
            if (iconImage == null)
                Debug.LogError("OptionItem: Image component not found in children!");
        }
    }

    /// <summary>
    /// OptionItem'ı başlatır ve gerekli bilgileri atar
    /// </summary>

    public void Setup(Sprite icon, int index, CharacterCreationManager creationManager, string style = null,int palette = 0)
    {
        if (iconImage == null)
        {
            Debug.LogError("OptionItem: iconImage is null during Setup!");
            return;
        }

        //PrintHierarchy(creationManager.previewInstance);

        optionIndex = index;
        manager = creationManager;
        managerCategory = manager.currentCategory;
        styleKey = style?.ToLower(); // null olabilir


        Color fixedColor;
        // Kategoriye göre davran
        switch (managerCategory)
        {
            case EnumCharacterCustomizationCategory.Skin:
                iconImage.sprite = icon;
                iconImage.color = manager.skinColors[index]; // ✅ Sadece Skin için renk uygula
                fixedColor = iconImage.color;
                fixedColor.a = 1f;
                iconImage.color = fixedColor;
                break;
            
            default:
                iconImage.sprite = icon;
                iconImage.color = Color.white; // Diğer kategorilerde sprite'ı tam göster
                fixedColor = iconImage.color;
                fixedColor.a = 1f;
                iconImage.color = fixedColor;
                break;
        }
        GetComponent<Button>().onClick.AddListener(OnClick);

        //Palette seçildiyse uygulanır.
        if(palette == 1)
        {
            iconImage.sprite = icon;
            iconImage.color = manager.colorValue[index]; // ✅ secilen Item için renk uygulanır
            fixedColor = iconImage.color;
            fixedColor.a = 1f;
            iconImage.color = fixedColor;
            this.colorFlag = true;
        }
        //Palette seçildiyse uygulanır.

    }

    /// <summary>
    /// Kullanıcı bu seçeneğe tıkladığında ilgili karakter özelliğini günceller
    /// </summary>

    public void OnClick()
    {
        if(this.colorFlag == true)
        {
            manager.dynamicCategoryManager.OnOptionItemClicked(this); 
            //this.colorFlag = false;
            manager.SelectColorPalette(optionIndex);

            return;
        }

        switch (managerCategory)
        {
            case EnumCharacterCustomizationCategory.Skin:
                manager.SelectSkinColor(optionIndex);
                break;

            case EnumCharacterCustomizationCategory.Hair_Boy:
            case EnumCharacterCustomizationCategory.Hair_Girl:
            case EnumCharacterCustomizationCategory.Hair_Mixed:
                if (!string.IsNullOrEmpty(styleKey))
                    manager.SelectHair(optionIndex, styleKey);
                else
                    Debug.LogWarning("OptionItem: styleKey is missing for hair category.");
                break;

            case EnumCharacterCustomizationCategory.Clothes:
                manager.SelectClothes(optionIndex, styleKey);
                break;

            case EnumCharacterCustomizationCategory.Accessory:
                manager.SelectAccessory(optionIndex, styleKey);
                break;

            case EnumCharacterCustomizationCategory.Hats:
                //manager.SelectHat(optionIndex, styleKey);
                break;

            case EnumCharacterCustomizationCategory.Beard:
            case EnumCharacterCustomizationCategory.Noise:
            case EnumCharacterCustomizationCategory.Freckle:
                //manager.SelectOutfit(optionIndex, styleKey);
                break;

            case EnumCharacterCustomizationCategory.Eyes:
            case EnumCharacterCustomizationCategory.EyeBrown:
                //manager.SelectAccessory(optionIndex, styleKey);
                break;

            default:
                Debug.LogWarning($"OptionItem: Unhandled category {managerCategory}");
                break;
        }

        //ToneSliderArea için
        manager.dynamicCategoryManager.OnOptionItemClicked(this); 
        //ToneSliderArea için
    }

    // Yeni bir CharacterPreview Item seçilirse (Aynı Item ise rengi koruması için yapılıyor)
    public void updateNewItemUpdateColorPalette(Transform colorRoot)
    {
        Color fixedColor;

        Image rootImage = colorRoot.GetComponent<Image>();
        iconImage.color = rootImage.color; // ✅renk uygula
        fixedColor = iconImage.color;
        fixedColor.a = 1f;
        iconImage.color = fixedColor;
    }

    void PrintHierarchy(GameObject obj)
    {
        Transform current = obj.transform;
        string hierarchy = current.name;

        while (current.parent != null)
        {
            current = current.parent;
            hierarchy = current.name + " → " + hierarchy;
        }

        Debug.Log("OptionItem Tam hiyerarşi: " + hierarchy);
    }


    //----ToneSliderArea için
    public Color GetColor()
    {
        //return GetComponent<Image>().color; // veya kendi tanımladığın renk alanı
        return iconImage.color;
    }

    public void SetPreviewColor(Color newColor)
    {
        GetComponent<Image>().color = newColor;
    }

    //----ToneSliderArea için
}
