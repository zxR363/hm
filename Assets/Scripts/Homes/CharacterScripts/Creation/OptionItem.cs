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
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    public void OnClick()
    {
        switch (managerCategory)
        {
            case EnumCharacterCustomizationCategory.Hair:
                manager.SelectHair(optionIndex);
                break;
            case EnumCharacterCustomizationCategory.Outfit:
                manager.SelectOutfit(optionIndex);
                break;
            case EnumCharacterCustomizationCategory.Skin:
                manager.SelectSkinColor(optionIndex);
                break;
            case EnumCharacterCustomizationCategory.Accessories:
                manager.SelectAccessory(optionIndex);
                break;
        }
    }
}
