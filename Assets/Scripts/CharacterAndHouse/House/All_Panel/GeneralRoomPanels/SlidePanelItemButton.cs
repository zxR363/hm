using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
                // ValidateAndCleanUp() is already called by ResetAll() above
                ToggleDragHandlers(false);
                ItemSelectionPanelController.Instance.ClosePanel();
            }
        }
    }

    /// <summary>
    /// Validates all active items in the scene.
    /// Uses an iterative "Stability Loop" to handle cascading reverts (e.g., A moves back to X, pushing B out).
    /// </summary>
    public static void ValidateAndCleanUp()
    {
        // Find all active handlers in the scene
        // We use a query potentially multiple times or cache it? caching is better if list doesn't change drastically (destroys remove from scene but list ref stays)
        // Actually, since we might destroy objects, we should be careful.
        // But FindObjectsOfType returns a snapshot array.
        
        int maxIterations = 5;
        
        for (int i = 0; i < maxIterations; i++)
        {
            bool anyChange = false;
            
            // 1. Refresh list (in case of destroys) and Force Validation
            // CRITICAL FIX: Use (true) to find Inactive DragHandlers. 
            // The scripts might be disabled by SlidePanelItemButton.ToggleDragHandlers, but we still need to validate their positions!
            var handlers = FindObjectsOfType<DragHandler>(true).Where(h => h.gameObject.activeInHierarchy).ToList();
            Debug.Log($"[ValidateAndCleanUp] Iteration {i+1}. Found {handlers.Count} handlers.");
            
            // SYNC PHYSICS: Ensure all previous moves are registered in the physics world/colliders
            Physics2D.SyncTransforms();

            foreach (var h in handlers)
            {
                // Force validation logic to update IsValidPlacement status
                // If the component is disabled, OnEnable/Update won't run, so this is crucial.
                h.ForceValidation();
            }

            // 2. Resolve Conflicts
            foreach (var handler in handlers)
            {
                // Skip if destroyed in previous inner loop (though we refreshed list, so safe for this pass)
                if (handler == null) continue;

                if (!handler.IsValidPlacement)
                {
                    Debug.Log($"[ValidateAndCleanUp] Conflict Found: {handler.name} is Invalid.");
                    Vector3 beforePos = handler.transform.localPosition;
                    
                    if (handler.TryResetPosition())
                    {
                        // Check if it actually moved (distance check)
                        if (Vector3.Distance(beforePos, handler.transform.localPosition) > 0.01f)
                        {
                            Debug.Log($"[ValidateAndCleanUp] Item {handler.name} moved to resolve conflict.");
                            anyChange = true;
                            handler.UpdateCurrentPositionAsValid();
                            // SYNC IMMEDIATELY: So the next item in this loop sees the empty spot!
                            Physics2D.SyncTransforms();
                        }
                    }
                    else
                    {
                        // Revert failed (No history), needs to be removed
                        Debug.Log($"[ValidateAndCleanUp] Item {handler.name} destroyed (No valid history).");
                        Destroy(handler.gameObject);
                        anyChange = true;
                    }
                }
                else
                {
                    // USER REQUEST: Update persistence here. If it survived validation, it's safe.
                    //handler.UpdateCurrentPositionAsValid();
                    // Debug.Log($"[ValidateAndCleanUp] Item {handler.name} is Valid.");
                }
            }

            // If no changes occurred this pass, we are stable.
            if (!anyChange)
            {
                // Debug.Log($"[ValidateAndCleanUp] Stability reached at iteration {i}.");
                break;
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
        // One global validation pass for efficiency
        ValidateAndCleanUp();

        for (int i = allButtons.Count - 1; i >= 0; i--)
        {
            if (allButtons[i] != null)
            {
                allButtons[i].ResetScale();
            }
            else
            {
                allButtons.RemoveAt(i);
            }
        }
    }
}
