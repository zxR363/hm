using UnityEngine;
using UnityEngine.UI;

namespace AvatarWorld.Interaction
{
    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(DragHandler))]
    public class ClothingItem : MonoBehaviour
    {
        [Header("Clothing Data")]
        public EnumCharacterCustomizationCategory category; // Hat, Clothes, Accessory
        public Sprite itemSprite; // The sprite that will be applied to the character

        private Image _image;

        private void Start()
        {
            _image = GetComponent<Image>();
            
            // If sprite is assigned manually, ensure the Image matches it
            if (itemSprite != null && _image.sprite != itemSprite)
            {
                _image.sprite = itemSprite;
            }
            // If Image has a sprite but itemSprite is null, capture it
            else if (_image.sprite != null && itemSprite == null)
            {
                itemSprite = _image.sprite;
            }
        }

        public void Initialize(EnumCharacterCustomizationCategory cat, Sprite sprite)
        {
            category = cat;
            itemSprite = sprite;
            
            if (_image == null) _image = GetComponent<Image>();
            if (_image != null) _image.sprite = sprite;
        }
    }
}
