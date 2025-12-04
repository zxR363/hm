using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TabButton : MonoBehaviour, UnityEngine.EventSystems.IPointerClickHandler, UnityEngine.EventSystems.IBeginDragHandler, UnityEngine.EventSystems.IDragHandler, UnityEngine.EventSystems.IEndDragHandler
{
    [SerializeField] private int tabIndex;
    public int TabIndex => tabIndex; // Public property for access

    private ItemSelectionPanelController controller;
    private CanvasGroup canvasGroup;
    private bool isDragging = false;

    public void Initialize(ItemSelectionPanelController ctrl)
    {
        // Debug.Log("ITEMSELECTION INJECT EDILDI");
        this.controller = ctrl;
    }
    
    // Called by Unity EventSystem (requires GraphicRaycaster)
    public void OnPointerClick(UnityEngine.EventSystems.PointerEventData eventData)
    {
        if (isDragging) return; // Ignore click if we were dragging

        if (controller != null)
        {
            controller.SelectTab(tabIndex);
        }
    }

    // Implement Drag interfaces to detect scrolling
    public void OnBeginDrag(UnityEngine.EventSystems.PointerEventData eventData)
    {
        isDragging = true;
        // Pass drag event to parent ScrollRect if possible
        ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, ExecuteEvents.beginDragHandler);
    }

    public void OnDrag(UnityEngine.EventSystems.PointerEventData eventData)
    {
        isDragging = true;
        ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, ExecuteEvents.dragHandler);
    }

    public void OnEndDrag(UnityEngine.EventSystems.PointerEventData eventData)
    {
        isDragging = false;
        ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, ExecuteEvents.endDragHandler);
    }

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    private void Start()
    {
        // Ensure we have a raycast target
        UnityEngine.UI.Image img = GetComponent<UnityEngine.UI.Image>();
        if (img == null)
        {
            img = gameObject.AddComponent<UnityEngine.UI.Image>();
            img.color = new Color(0, 0, 0, 0.004f); // Almost transparent
        }
        img.raycastTarget = true;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
        }
    }
}
