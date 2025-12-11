using System.Collections.Generic;
using UnityEngine;

public class SlidePanelItemButton : MonoBehaviour
{
    [SerializeField] private GameObject itemBehaviourDragAndOutline; //ItemBehaviour  aktif edilecek ana obje(RoomPanel içeren content)

    private static List<SlidePanelItemButton> allButtons = new List<SlidePanelItemButton>();
    private Vector3 initialScale;

    private void Awake()
    {
        initialScale = transform.localScale;
        if (!allButtons.Contains(this))
        {
            allButtons.Add(this);
        }
        ToggleDragHandlers(false);
    }

    private void OnDestroy()
    {
        if (allButtons.Contains(this))
        {
            allButtons.Remove(this);
        }
    }



    public void OnClick()
    {
        // Check if this button is already scaled up (active)
        // Using a small epsilon for float comparison
        bool isAlreadyActive = transform.localScale.x > initialScale.x * 1.05f;

        // Reset all other buttons first
        ResetAll();

        if (ItemSelectionPanelController.Instance != null)
        {
            // If it wasn't active, scale it up and OPEN the panel
            if (!isAlreadyActive)
            {
                transform.localScale = initialScale * 1.15f;
                ToggleDragHandlers(true);
                ItemSelectionPanelController.Instance.OpenPanel();

            }
            // If it was active, it stays reset (toggled off) and CLOSE the panel
            else
            {
                ValidateAndCleanUp();
                ToggleDragHandlers(false);
                ItemSelectionPanelController.Instance.ClosePanel();
            }
        }
    }

    private void ValidateAndCleanUp()
    {
        Debug.Log("ValidateAndCleanUP");
        if (itemBehaviourDragAndOutline == null) return;

        // Check if any DragHandler on the item is in an invalid state
        DragHandler[] handlers = itemBehaviourDragAndOutline.GetComponentsInChildren<DragHandler>(true);
        Debug.Log($"[SlidePanelItemButton] Validating {handlers.Length} handlers on {itemBehaviourDragAndOutline.name}");

        foreach (var handler in handlers)
        {
            handler.ForceValidation(); // Make sure the state is up to date!
            
            Debug.Log($"[SlidePanelItemButton] Checking Handler: {handler.name}, IsValid: {handler.IsValidPlacement}");
            if (!handler.IsValidPlacement)
            {
                Debug.Log($"[SlidePanelItemButton] Item {handler.name} is in INVALID placement. Attempting Revert.");
                // Try to Revert to a safe history position.
                // If fails (New Item with no history), we Destroy it (Clean Cleanup).
                if (!handler.TryResetPosition())
                {
                    Debug.Log($"[SlidePanelItemButton] Item {handler.name} has no valid history. Destroying.");
                    Destroy(handler.gameObject);
                }
            }
        }
    }

    //Itemların Drag yapılabilmesi ve UIStickerEffect(Outline) aktif edilmesi için gerekli unsurlar
    private void ToggleDragHandlers(bool enable)
    {
        if (itemBehaviourDragAndOutline == null) return;

        //Find all IItemBehaviours in the container and its children
        IItemBehaviours[] items = itemBehaviourDragAndOutline.GetComponentsInChildren<IItemBehaviours>(true);
        foreach (var item in items)
        {
            DragHandler dragHandler = item.GetComponent<DragHandler>();
            if (dragHandler != null)
            {
                dragHandler.enabled = enable;
            }
        }

        //Find all UIStickerEffects in the container and its children
        UIStickerEffect[] stickerEffects = itemBehaviourDragAndOutline.GetComponentsInChildren<UIStickerEffect>(true);
        foreach (var stickerEffect in stickerEffects)
        {
            stickerEffect.enabled = enable;
        }

    }

    public void ResetScale()
    {
        transform.localScale = initialScale;
        ToggleDragHandlers(false);
    }

    public static void ResetAll()
    {
        
        for (int i = allButtons.Count - 1; i >= 0; i--)
        {
            if (allButtons[i] != null)
            {
                // Ensure we clean up any invalid items associated with this button before closing/resetting
                allButtons[i].ValidateAndCleanUp();
                allButtons[i].ResetScale();
            }
            else
            {
                allButtons.RemoveAt(i);
            }
        }
    }
}
