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
                ToggleDragHandlers(false);
                ItemSelectionPanelController.Instance.ClosePanel();
            }
        }
    }

    private void ToggleDragHandlers(bool enable)
    {
        if (itemBehaviourDragAndOutline == null) return;

        //Find all IItemBehaviours in the container and its children
        IItemBehaviours[] items = itemBehaviourDragAndOutline.GetComponentsInChildren<IItemBehaviours>(true);
        Debug.Log("BAKLIM");
        foreach (var item in items)
        {
            DragHandler dragHandler = item.GetComponent<DragHandler>();
            if (dragHandler != null)
            {
                Debug.Log("DragHandler enabled: " + enable);
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
                allButtons[i].ResetScale();
            }
            else
            {
                allButtons.RemoveAt(i);
            }
        }
    }
}
