using UnityEngine;
using UnityEngine.UI;

public class RoomObjectInteraction : MonoBehaviour, IInteractable
{
    [Header("Interaction Settings")]
    //İcerisine barındırıyor mu diye bakıyor
    [SerializeField] private string requiredToolNamePart = "PaintBrush"; // Name part to check (e.g. "PaintBrush")
    [SerializeField] private Sprite newSprite;
    [SerializeField] private bool destroyTool = false; // Should the tool be destroyed after use?

    public bool CanInteract(RoomObject sourceItem)
    {
        Debug.Log("INTERACTION TETIKLENDI");
        if (sourceItem == null) return false;
        
        // Simple check: does the dropped item's name contain the required string?
        // You could replace this with an ID system or ItemType enum later.
        return sourceItem.name.Contains(requiredToolNamePart);
    }

    public string CurrentSourceItemName { get; private set; }

    public bool OnInteract(RoomObject sourceItem)
    {
        Sprite spriteToUse = newSprite; // Default
        string sourceName = System.Text.RegularExpressions.Regex.Replace(sourceItem.name.Replace("(Clone)", ""), @"\s*\(\d+\)|\s+\d+$", "").Trim();
        
        // PERSISTENCE: If we have the value, use the FULL PATH
        // PERSISTENCE: If we have the value, use the FULL PATH
        if (!string.IsNullOrEmpty(sourceItem.loadedFromResourcePath))
        {
            // Generic Fix: Force "Items" folder standard relative to parent.
            // Assumption: The physical assets are always in ".../Items/" regardless of the specific Tab name (e.g. "FurnitureContent").
            string basePath = sourceItem.loadedFromResourcePath;
            int lastSlashIndex = basePath.LastIndexOf('/');
            
            if (lastSlashIndex >= 0)
            {
                 // We want to replace the folder name containing the item (e.g. WallAndFloorContent) with "Items"
                 // Current Path: .../HouseScene/WallAndFloorContent/Wall1
                 // Desired Path: .../HouseScene/Items/Wall1
                 
                 // 1. Get the directory path (excluding filename)
                 string directoryPath = basePath.Substring(0, lastSlashIndex); 
                 
                 // 2. Find the parent of this directory
                 int parentSlashIndex = directoryPath.LastIndexOf('/');
                 
                 if (parentSlashIndex >= 0)
                 {
                     // extract ".../HouseScene/"
                     string parentPath = directoryPath.Substring(0, parentSlashIndex + 1);
                     basePath = parentPath + "Items";
                 }
                 else
                 {
                     // Fallback if at root
                     basePath = "Items";
                 }
            }
            // If no slash, it's a top-level folder? Unlikely, but just use as is or append /Items?
            // Let's assume there's always a Scene folder before it.

            CurrentSourceItemName = basePath + "/" + sourceName;
            Debug.Log($"[RoomObjectInteraction] Using Dynamic Full Path (Generic Corrected): {CurrentSourceItemName}");
        }
        else
        {
            CurrentSourceItemName = sourceName; 
            Debug.Log($"[RoomObjectInteraction] Using Simple Name (Legacy): {CurrentSourceItemName}");
        }
        
        Debug.Log($"[RoomObjectInteraction] Interacting with {sourceName}. Name saved.");

        // Check for Image or RawImage on Root
        if (sourceItem.GetComponent<Image>() != null && sourceItem.GetComponent<Image>().sprite != null)
        {
            spriteToUse = sourceItem.GetComponent<Image>().sprite;
            // CurrentSourceItemName = sourceName; // FIXED: Do not overwrite the full path!
            Debug.Log($"[RoomObjectInteraction] Found Sprite on Root of {sourceName}");
        }
        else if (sourceItem.GetComponent<RawImage>() != null && sourceItem.GetComponent<RawImage>().texture != null)
        {
             // If source is RawImage, we can't easily get a Sprite unless we create one, 
             // but for now let's assume valid interaction and just save the name so we can try loading Prefab later.
             // But wait, RestoreState logic relies on Sprite.
             // If the Prefab has a RawImage, we might need a Texture?
             // Let's stick to Sprite for now, but save the name anyway if it looks like a visual item.
             // CurrentSourceItemName = sourceName; // FIXED: Do not overwrite full path!
             Debug.Log($"[RoomObjectInteraction] Found RawImage on Root of {sourceName}. Saving name to restore from Prefab.");
        }
        else
        {
             // Check children for Image
            var childImage = sourceItem.GetComponentInChildren<Image>();
            if (childImage != null && childImage.sprite != null)
            {
                spriteToUse = childImage.sprite;
                //Debug.Log($"[RoomObjectInteraction] Found Sprite on Child of {sourceName}");
            }
            else
            {
                var childRaw = sourceItem.GetComponentInChildren<RawImage>();
                //Debug.Log($"[RoomObjectInteraction] Found RawImage on Child of {sourceName}? {childRaw != null}");
            }
        }
        
        // VISUAL APPLY (Runtime only)
        bool visualApplied = false;
        
        // 1. Try applying to our Image
        Image myImage = GetComponent<Image>();
        if (myImage != null)
        {
            if (spriteToUse != null)
            {
                myImage.sprite = spriteToUse;
                visualApplied = true;
            }
            // If we have a RawImage source but an Image target, we can't easily assign Texture->Sprite at runtime without conversion.
            // But we SAVED the name, so RestoreState can handle it properly by loading the asset.
        }
        
        // 2. Try applying to our RawImage
        if (!visualApplied)
        {
            RawImage myRaw = GetComponent<RawImage>();
            if (myRaw != null)
            {
                 // If source provided a sprite, we can use it on RawImage
                 if (spriteToUse != null)
                 {
                     myRaw.texture = spriteToUse.texture;
                     visualApplied = true;
                 }
                 // If source was RawImage (no sprite), we need to extract texture manually
                 else 
                 {
                     Texture srcTex = ExtractTexture(sourceItem.gameObject);
                     if (srcTex != null)
                     {
                         myRaw.texture = srcTex;
                         visualApplied = true;
                         //Debug.Log($"[SpriteChanger] Texture updated on {name} (RawImage) using texture from {sourceName}");
                     }
                 }
            }
        }

        if (visualApplied || !string.IsNullOrEmpty(CurrentSourceItemName))
        {
            //Even if visual update fails (e.g. Texture->Image mismatch), we entered a valid interaction state and saved the name.
            if (!visualApplied) 
                Debug.LogWarning($"[RoomObjectInteraction] Interaction valid (Name: {sourceName}), but visual update failed (Type Mismatch?). Persistence will fix this on load.");
            else
                Debug.Log($"[RoomObjectInteraction] Interaction Valid. CurrentSourceItemName='{CurrentSourceItemName}'. Triggering NotifyChange.");

            _isInteracted = true;
            
            // Notify RoomPanel to save this change
            RoomObject roomObj = GetComponent<RoomObject>();
            if (roomObj != null)
            {
                roomObj.NotifyChange(true); // Save Immediately
            }
            
            if (destroyTool)
            {
                Destroy(sourceItem.gameObject);
            }
            return true;
        }

        return false;
    }

    private bool _isInteracted = false;
    public bool IsInteracted => _isInteracted;

    public void RestoreState(bool interacted, string sourceItemName = null)
    {
        _isInteracted = interacted;
        if (_isInteracted && !string.IsNullOrEmpty(sourceItemName))
        {
             // Try immediate restore
             bool success = TryRestoreFromSource(sourceItemName);
             
             // If failed, it might be due to Race Condition (UI not ready). Retry later.
             if (!success)
             {
                 StartCoroutine(RetryRestoreRoutine(sourceItemName));
             }
        }
    }
    
    private System.Collections.IEnumerator RetryRestoreRoutine(string sourceItemName)
    {
        // Wait a bit for UI to initialize
        yield return new WaitForSeconds(0.5f);
        if (TryRestoreFromSource(sourceItemName)) yield break;
        
        yield return new WaitForSeconds(1.0f);
        if (TryRestoreFromSource(sourceItemName)) yield break;
        
        Debug.LogError($"[RoomObjectInteraction] Final Retry Failed: Could not restore '{sourceItemName}' from Resources or Scene.");
    }

    private bool TryRestoreFromSource(string sourceItemName)
    {
        Sprite spriteToUse = newSprite;
        Texture textureToUse = (newSprite != null) ? newSprite.texture : null;
        bool found = false;

        // 1. Try Resources
        GameObject prefab = Resources.Load<GameObject>(sourceItemName);
        Debug.Log("BAKALIM)="+sourceItemName);
        if (prefab == null) prefab = Resources.Load<GameObject>($"{sourceItemName}");
        if (prefab == null) prefab = Resources.Load<GameObject>($"Prefabs/{sourceItemName}");
        

        // Specific Paths (REQUIRED for Legacy "Name-Only" Saves like 'TestWall')
        // if (prefab == null) prefab = Resources.Load<GameObject>($"Prefabs/RoomItems/HouseScene/{sourceItemName}");
        // if (prefab == null) prefab = Resources.Load<GameObject>($"Prefabs/RoomItems/HouseScene/Items/{sourceItemName}");
        // if (prefab == null) prefab = Resources.Load<GameObject>($"Prefabs/RoomItems/HouseScene/DecorContent/{sourceItemName}");
        // if (prefab == null) prefab = Resources.Load<GameObject>($"Prefabs/RoomItems/HouseScene/FurnitureContent/{sourceItemName}");
        // if (prefab == null) prefab = Resources.Load<GameObject>($"Prefabs/RoomItems/{sourceItemName}");

        if (prefab == null)
        {
            Debug.LogWarning($"[RoomObjectInteraction] RestoreState: Failed to find '{sourceItemName}' in Resources. Tried: Prefabs/, RoomItems/HouseScene/(Items|DecorContent|FurnitureContent)/...");
        }

        if (prefab != null)
        {
            // Try getting Sprite
            Sprite s = ExtractSprite(prefab);
            if (s != null)
            {
                spriteToUse = s;
                textureToUse = s.texture;
                found = true;
                Debug.Log($"[RoomObjectInteraction] RestoreState: Loaded Sprite '{sourceItemName}' from Resources.");
            }
            // Try getting Texture
            else
            {
                Texture t = ExtractTexture(prefab);
                if (t != null)
                {
                    textureToUse = t;
                    spriteToUse = null; 
                    found = true;
                    Debug.Log($"[RoomObjectInteraction] RestoreState: Loaded Texture '{sourceItemName}' from Resources.");
                }
            }
        }

        // 2. Fallback: Search in Scene UI
        if (!found)
        {
            ItemDragPanel[] panels = Resources.FindObjectsOfTypeAll<ItemDragPanel>(); 
            if (panels.Length > 0)
            {
                foreach (var panel in panels)
                {
                    if (panel.ItemPrefab != null && panel.ItemPrefab.name == sourceItemName)
                    {
                        Sprite s = ExtractSprite(panel.ItemPrefab);
                        if (s != null)
                        {
                            spriteToUse = s;
                            textureToUse = s.texture;
                            found = true;
                            Debug.Log($"[RoomObjectInteraction] Restored Sprite '{sourceItemName}' from Scene UI.");
                            break;
                        }
                        
                        Texture t = ExtractTexture(panel.ItemPrefab);
                        if (t != null)
                        {
                            textureToUse = t;
                            spriteToUse = null;
                            found = true;
                            Debug.Log($"[RoomObjectInteraction] Restored Texture '{sourceItemName}' from Scene UI.");
                            break;
                        }
                    }
                }
            }
            // If panels.Length is 0, we can't assume failure yet if we are in Retry loop vs First Attempt.
            // But this method just reports success/failure of THIS attempt.
        }

        if (found)
        {
            CurrentSourceItemName = sourceItemName;
            
            // Apply
            Image img = GetComponent<Image>();
            if (img != null)
            {
                if (spriteToUse != null) img.sprite = spriteToUse;
            }
            else
            {
                RawImage rawImg = GetComponent<RawImage>();
                if (rawImg != null)
                {
                    if (textureToUse != null) rawImg.texture = textureToUse;
                }
            }
            return true;
        }
        
        return false;
    }

    private Sprite ExtractSprite(GameObject obj)
    {
        Image img = obj.GetComponent<Image>();
        if (img != null && img.sprite != null) return img.sprite;
        
        img = obj.GetComponentInChildren<Image>();
        if (img != null && img.sprite != null) return img.sprite;

        return null;
    }
    
    private Texture ExtractTexture(GameObject obj)
    {
        RawImage raw = obj.GetComponent<RawImage>();
        if (raw != null && raw.texture != null) return raw.texture;
        
        raw = obj.GetComponentInChildren<RawImage>();
        if (raw != null && raw.texture != null) return raw.texture;
        
        return null;
    }
}
