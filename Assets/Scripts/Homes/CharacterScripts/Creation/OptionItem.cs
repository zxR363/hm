using UnityEngine;
using UnityEngine.UI;

public class OptionItem : MonoBehaviour
{
    public Image iconImage;

    private int optionIndex;
    private CharacterCreationManager manager;
    private EnumCharacterCustomizationCategory managerCategory;

    void Awake()
    {
        if (iconImage == null)
        {
            iconImage = GetComponentInChildren<Image>();
            if (iconImage == null)
                Debug.LogError("OptionItem: Image component not found in children!");
        }
    }

    public void Setup(Sprite icon, int index, CharacterCreationManager creationManager)
    {
        if (iconImage == null)
        {
            Debug.LogError("OptionItem: iconImage is null during Setup!");
            return;
        }

        optionIndex = index;
        manager = creationManager;
        managerCategory = manager.currentCategory;

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
    }

    public void OnClick()
    {
        switch (managerCategory)
        {
            case EnumCharacterCustomizationCategory.Skin:
                manager.SelectSkinColor(optionIndex);
                break;
            case EnumCharacterCustomizationCategory.Hair_Boy:
                manager.SelectHair(optionIndex,"boy");
                break;
            case EnumCharacterCustomizationCategory.Hair_Girl:
                manager.SelectHair(optionIndex,"girl");
                break;
            case EnumCharacterCustomizationCategory.Hair_Mixed:
                manager.SelectHair(optionIndex,"mixed");
                break;
            case EnumCharacterCustomizationCategory.Outfit:
                manager.SelectOutfit(optionIndex);
                break;
            case EnumCharacterCustomizationCategory.Accessories:
                manager.SelectAccessory(optionIndex);
                break;
        }
    }
}
