using UnityEngine;
using UnityEngine.UI;

public class OptionItem : MonoBehaviour
{
    public Image iconImage;

    private int optionIndex;
    private CharacterCreationManager manager;
    private EnumCharacterCustomizationCategory managerCategory;
    private string styleKey; // 🔑 Alt klasör adı (örneğin: "casual", "glasses", "crowns")


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

    public void Setup(Sprite icon, int index, CharacterCreationManager creationManager, string style = null)
    {
        if (iconImage == null)
        {
            Debug.LogError("OptionItem: iconImage is null during Setup!");
            return;
        }

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

        Debug.Log("OptionItem Setup → currentCategory: " + manager.currentCategory);
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    /// <summary>
    /// Kullanıcı bu seçeneğe tıkladığında ilgili karakter özelliğini günceller
    /// </summary>

    public void OnClick()
    {
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

            case EnumCharacterCustomizationCategory.Accessories:
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

    }
}
