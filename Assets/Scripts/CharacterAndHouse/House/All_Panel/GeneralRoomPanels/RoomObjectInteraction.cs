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

    public bool OnInteract(RoomObject sourceItem)
    {
        if (newSprite != null)
        {
            // 1. Try standard Image component
            Image img = GetComponent<Image>();
            if (img != null)
            {
                img.sprite = newSprite;
                Debug.Log($"[SpriteChanger] Sprite updated on {name} using {sourceItem.name}");
            }
            else
            {
            // 2. Try RawImage component (since user mentioned using RawImage)
                RawImage rawImg = GetComponent<RawImage>();
                if (rawImg != null)
                {
                    rawImg.texture = newSprite.texture;
                    Debug.Log($"[SpriteChanger] Texture updated on {name} (RawImage) using {sourceItem.name}");
                }
                else
                {
                    Debug.LogWarning($"[SpriteChanger] No Image or RawImage component found on {name}!");
                }
            }
            
            _isInteracted = true;
            
            // Notify RoomObject that we changed, so RoomPanel can save us
            RoomObject roomObj = GetComponent<RoomObject>();
            if (roomObj != null)
            {
                roomObj.NotifyChange(true);
            }
            else
            {
                Debug.LogError($"[RoomObjectInteraction] CRITICAL: RoomObject component is MISSING on {name}! This object will NOT be saved.");
            }
        }

        if (destroyTool)
        {
            Destroy(sourceItem.gameObject);
            return true;
        }
        
        return false;
    }

    private bool _isInteracted = false;
    public bool IsInteracted => _isInteracted;

    public void RestoreState(bool interacted)
    {
        _isInteracted = interacted;
        if (_isInteracted && newSprite != null)
        {
             // Apply the sprite/texture immediately without interaction logic
            Image img = GetComponent<Image>();
            if (img != null)
            {
                img.sprite = newSprite;
            }
            else
            {
                RawImage rawImg = GetComponent<RawImage>();
                if (rawImg != null)
                {
                    rawImg.texture = newSprite.texture;
                }
            }
        }
    }
}
