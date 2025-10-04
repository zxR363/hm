using UnityEngine;
using UnityEngine.EventSystems;
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

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Aktif RoomPanel'i bul
        RoomPanel[] roomPanels = FindObjectsOfType<RoomPanel>()
            .Where(p => p.gameObject.activeInHierarchy)
            .ToArray();

        if (roomPanels.Length > 0)
        {
            dragRoot = roomPanels[0].transform;
            canvas = dragRoot.GetComponentInParent<Canvas>();
            Debug.Log("DragRoot seçildi: " + dragRoot.name);
        }
        else
        {
            Debug.LogWarning("Aktif RoomPanel bulunamadı.");
            return;
        }

        // Geçici görsel kopya oluştur
        dragGhost = Instantiate(itemPrefab, transform.position, Quaternion.identity, dragRoot);

        RectTransform ghostRT = dragGhost.GetComponent<RectTransform>();
        ghostRT.localScale = originalScale;
        ghostRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, originalSizeDelta.x * 1.1f);
        ghostRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, originalSizeDelta.y * 1.1f);

        CanvasGroup ghostCG = dragGhost.GetComponent<CanvasGroup>();
        ghostCG.alpha = 0.6f;
        ghostCG.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
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

    public void OnEndDrag(PointerEventData eventData)
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
        dragGhost = null;

        Debug.Log("Yeni item oluşturuldu → drag tamamlandı.");
    }
}