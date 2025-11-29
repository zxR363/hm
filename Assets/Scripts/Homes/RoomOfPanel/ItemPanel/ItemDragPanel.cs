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
        // Aktif RoomPanel'i bul
        RoomPanel[] roomPanels = FindObjectsOfType<RoomPanel>()
            .Where(p => p.gameObject.activeInHierarchy)
            .ToArray();

        if (roomPanels.Length > 0)
        {
            dragRoot = roomPanels[0].transform;
            canvas = dragRoot.GetComponentInParent<Canvas>();
        }
        else
        {
            Debug.LogWarning("Aktif RoomPanel bulunamadı.");
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

        CanvasGroup ghostCG = dragGhost.GetComponent<CanvasGroup>();
        ghostCG.alpha = 1f;
        ghostCG.blocksRaycasts = true;

        RectTransform ghostRT = dragGhost.GetComponent<RectTransform>();
        ghostRT.localScale = originalScale;
        ghostRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, originalSizeDelta.x);
        ghostRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, originalSizeDelta.y);

        dragGhost.transform.SetParent(dragRoot, false);
        
        // Register logic if needed
        RoomPanel panel = dragRoot.GetComponent<RoomPanel>();
        if (panel != null)
        {
             if (dragGhost.GetComponent<RoomObject>() == null)
                dragGhost.AddComponent<RoomObject>();
        }

        // Reset sorting order to default (so it doesn't stay at UI layer)
        ItemSelection itemSelection = dragGhost.GetComponent<ItemSelection>();
        if (itemSelection != null)
        {
            itemSelection.ResetSortingOrder();
        }

        dragGhost = null;
        Debug.Log("Yeni item oluşturuldu → drag tamamlandı.");
    }
}