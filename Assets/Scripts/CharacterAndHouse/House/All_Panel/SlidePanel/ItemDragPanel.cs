using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;

public class ItemDragPanel : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;

    [SerializeField] private Vector3 originalScale;
    [SerializeField] private Vector2 originalSizeDelta;

    [SerializeField] private GameObject itemPrefab; // spawn edilecek yeni item
    private GameObject dragGhost; // geçici görsel kopya
    private Transform dragRoot;   // aktif RoomPanel
    
    // Scroll handling
    private ScrollRect parentScrollRect;
    private bool isScrolling = false;
    private bool isDraggingItem = false;
    private bool isDirectionDecided = false;
    
    [Header("Settings")]
    [SerializeField] private float dragThreshold = 5f; // Pixels to move before deciding (Lower = more sensitive)

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        // Find parent ScrollRect
        parentScrollRect = GetComponentInParent<ScrollRect>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isScrolling = false;
        isDraggingItem = false;
        isDirectionDecided = false;
        
        // Pass to ScrollRect just in case, but we might cancel it later if we decide to drag item
        if (parentScrollRect != null) parentScrollRect.OnBeginDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDirectionDecided)
        {
            Vector2 delta = eventData.position - eventData.pressPosition;
            if (delta.magnitude > dragThreshold)
            {
                isDirectionDecided = true;
                if (Mathf.Abs(delta.y) > Mathf.Abs(delta.x))
                {
                    // Vertical -> Scroll
                    isScrolling = true;
                    isDraggingItem = false;
                }
                else
                {
                    // Horizontal -> Item Drag
                    isScrolling = false;
                    isDraggingItem = true;
                    StartItemDrag();
                }
            }
            else
            {
                // Not moved enough yet, pass to scrollrect just to keep it responsive?
                // Or wait? Usually waiting is better to avoid jitter.
                // But ScrollRect might need immediate updates. 
                // Let's pass to ScrollRect tentatively.
                if (parentScrollRect != null) parentScrollRect.OnDrag(eventData);
            }
            return;
        }

        if (isScrolling)
        {
            if (parentScrollRect != null) parentScrollRect.OnDrag(eventData);
        }
        else if (isDraggingItem)
        {
            UpdateItemDrag(eventData);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isScrolling)
        {
            if (parentScrollRect != null) parentScrollRect.OnEndDrag(eventData);
        }
        else if (isDraggingItem)
        {
            EndItemDrag();
        }
        
        isScrolling = false;
        isDraggingItem = false;
        isDirectionDecided = false;
    }

    private void StartItemDrag()
    {
        // Find the main canvas to drag on top of everything
        canvas = GetComponentInParent<Canvas>();
        if (canvas != null && canvas.rootCanvas != null)
        {
            canvas = canvas.rootCanvas; // Use root canvas
            dragRoot = canvas.transform;
        }
        else
        {
            Debug.LogWarning("Canvas bulunamadı.");
            isDraggingItem = false;
            return;
        }

        // Geçici görsel kopya oluştur
        dragGhost = Instantiate(itemPrefab, transform.position, Quaternion.identity, dragRoot);

        RectTransform ghostRT = dragGhost.GetComponent<RectTransform>();
        ghostRT.localScale = originalScale;
        ghostRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, originalSizeDelta.x * 1.1f);
        ghostRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, originalSizeDelta.y * 1.1f);

        CanvasGroup ghostCG = dragGhost.GetComponent<CanvasGroup>();
        if (ghostCG == null) ghostCG = dragGhost.AddComponent<CanvasGroup>();
        ghostCG.alpha = 0.6f;
        ghostCG.blocksRaycasts = false;
    }

    private void UpdateItemDrag(PointerEventData eventData)
    {
        if (dragGhost == null || dragRoot == null || canvas == null) return;

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            dragRoot.GetComponent<RectTransform>(),
            eventData.position,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out localPoint
        );

        RectTransform ghostRT = dragGhost.GetComponent<RectTransform>();
        ghostRT.localPosition = localPoint;
        dragGhost.transform.SetAsLastSibling();
    }

    private void EndItemDrag()
    {
        if (dragGhost == null) return;

        // Find which RoomPanel we dropped onto
        RoomPanel targetPanel = null;
        RoomPanel[] roomPanels = FindObjectsOfType<RoomPanel>()
            .Where(p => p.gameObject.activeInHierarchy)
            .ToArray();

        foreach (var panel in roomPanels)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(
                panel.GetComponent<RectTransform>(), 
                Input.mousePosition, 
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera))
            {
                targetPanel = panel;
                break;
            }
        }

        if (targetPanel != null)
        {
            CanvasGroup ghostCG = dragGhost.GetComponent<CanvasGroup>();
            ghostCG.alpha = 1f;
            ghostCG.blocksRaycasts = true;

            RectTransform ghostRT = dragGhost.GetComponent<RectTransform>();
            ghostRT.localScale = originalScale;
            ghostRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, originalSizeDelta.x);
            ghostRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, originalSizeDelta.y);

            // Parent to the target room's object container (or transform if container is null)
            Transform targetContainer = targetPanel.objectContainer != null ? targetPanel.objectContainer : targetPanel.transform;
            dragGhost.transform.SetParent(targetContainer, true); // true to keep world position
            
            // Register logic
            if (dragGhost.GetComponent<RoomObject>() == null)
                dragGhost.AddComponent<RoomObject>();

            // Reset sorting order
            ItemSelection itemSelection = dragGhost.GetComponent<ItemSelection>();
            if (itemSelection != null)
            {
                itemSelection.ResetSortingOrder();
            }
            
            Debug.Log($"Item {targetPanel.name} paneline bırakıldı.");
        }
        else
        {
            // Dropped outside any room -> Cancel
            Destroy(dragGhost);
            Debug.Log("Item herhangi bir odaya bırakılmadı, iptal edildi.");
        }

        dragGhost = null;
    }
}