using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace AvatarWorld.Interaction
{
    public class CharacterClothingController : MonoBehaviour
    {
        [Header("Settings")]
        public GameObject clothingItemPrefab; // Prefab to spawn when unequipping (Must have ClothingItem + DragHandler)

        [Header("Part Names (Must match Hierarchy)")]
        public string hatPartName = "Hat"; // Or "Hats"
        public string clothesPartName = "Clothes";
        public string accessoryPartName = "Accessory";
        // Masks or Glasses? Usually under Accessory or Eyes? 
        // Using "Accessory" for now as per Enum.

        // Helper to map Enum to Part Name
        private string GetPartName(EnumCharacterCustomizationCategory category)
        {
            switch (category)
            {
                case EnumCharacterCustomizationCategory.Hats: return hatPartName;
                case EnumCharacterCustomizationCategory.Clothes: return clothesPartName;
                case EnumCharacterCustomizationCategory.Accessory: return accessoryPartName;
                default: return null;
            }
        }

        public bool TryEquip(ClothingItem newItem)
        {
            if (newItem == null)
            {
                Debug.LogError($"[CharacterClothing] TryEquip failed: newItem is null");
                return false;
            }

            string partName = GetPartName(newItem.category);
            Debug.Log($"[CharacterClothing] TryEquip: Category={newItem.category}, TargetPart={partName}");

            if (string.IsNullOrEmpty(partName)) 
            {
                Debug.LogError($"[CharacterClothing] TryEquip failed: No mapping for category {newItem.category}");
                return false;
            }

            Transform partTransform = FindPart(transform, partName);
            if (partTransform == null)
            {
                // Try plural/singular variations if not found (Hack for Hat/Hats confusion)
                if (newItem.category == EnumCharacterCustomizationCategory.Hats)
                    partTransform = FindPart(transform, "Hats");
                
                if (partTransform == null)
                {
                    Debug.LogError($"[CharacterClothing] Part '{partName}' NOT FOUND on {name}. Hierarchy issue?");
                    return false;
                }
            }
            Debug.Log($"[CharacterClothing] Found Part: {partTransform.name}");

            Image partImage = partTransform.GetComponent<Image>();
            if (partImage == null)
            {
                 Debug.LogError($"[CharacterClothing] Part '{partTransform.name}' has no Image component!");
                 return false;
            }

            // 1. UNEQUIP OLD (Swap Logic)
            // Only if the character is currently wearing something valid (not null/none)
            // We assume 'null' sprite means naked/default.
            if (partImage.sprite != null && clothingItemPrefab != null)
            {
                 Debug.Log($"[CharacterClothing] Unequipping old sprite: {partImage.sprite.name}");
                 // Spawn the old item into the world
                 SpawnClothingItem(newItem.category, partImage.sprite, newItem.transform.position);
            }

            // 2. EQUIP NEW
            partImage.sprite = newItem.itemSprite;
            Debug.Log($"[CharacterClothing] Applied new sprite: {newItem.itemSprite?.name}");

            // 3. APPLY SETTINGS (Offset/Scale from DB)
            // This is critical for preventing alignment issues.
            ImageSettingsApplier applier = partTransform.GetComponent<ImageSettingsApplier>();
            if (applier != null)
            {
                Debug.Log($"[CharacterClothing] Applying ImageSettings for {partTransform.name}");
                applier.ApplySettings();
            }
            else
            {
                Debug.LogWarning($"[CharacterClothing] No ImageSettingsApplier found on {partTransform.name}. Alignment might be wrong.");
            }

            return true; // Success
        }

        private void SpawnClothingItem(EnumCharacterCustomizationCategory category, Sprite sprite, Vector3 position)
        {
            // Spawn just slightly offset so it doesn't overlap perfectly with the new drop potentially
            GameObject obj = Instantiate(clothingItemPrefab, transform.parent); // Spawn in Room
            obj.transform.position = position;
            
            // Setup
            ClothingItem itemScript = obj.GetComponent<ClothingItem>();
            if (itemScript != null)
            {
                itemScript.Initialize(category, sprite);
            }

            // Ensure Scale is 1 (Prefabs might be weird)
            obj.transform.localScale = Vector3.one;
            
            // Add slight random offset to prevent stacking
            obj.transform.position += new Vector3(Random.Range(-50f, 50f), Random.Range(-50f, 50f), 0);
        }

        // Recursive Helper (Copied from CharacterCreationManager)
        private Transform FindPart(Transform root, string partName)
        {
            Transform t = root.Find(partName);
            if (t != null) return t;

            foreach (Transform child in root)
            {
                Transform result = FindPart(child, partName);
                if (result != null) return result;
            }
            return null;
        }
    }
}
