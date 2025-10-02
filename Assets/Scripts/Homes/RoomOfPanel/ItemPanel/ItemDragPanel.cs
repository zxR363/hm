using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;

public class ItemDragPanel : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Transform originalParent;
    private Vector3 worldPosBeforeDrag;

    [SerializeField] Vector3 originalScale;
    [SerializeField] Vector2 originalSizeDelta;

    private Canvas canvas;

    private Transform dragRoot; // HouseCanvas

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        worldPosBeforeDrag = transform.position;
        originalParent = transform.parent;

        transform.SetParent(dragRoot, false);
        transform.position = worldPosBeforeDrag;
        transform.localScale = originalScale;

        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {

        //RoomManager roomManager = FindObjectOfType<RoomManager>();
        //List<RoomPanel> roomPanels = RoomManager.Instance.roomPanelsList;
        RoomPanel[] allPanels = FindObjectsOfType<RoomPanel>();
        RoomPanel[] roomPanels = allPanels.Where(p => p.gameObject.activeInHierarchy).ToArray();

        if(allPanels !=null)
        {
            Debug.Log("PANEL GEZIYOR"+roomPanels);
            foreach (var panel in roomPanels)
            {   
                dragRoot = panel.gameObject.transform;
                canvas = dragRoot.GetComponentInParent<Canvas>();
                Debug.Log("ilgili panel root secildi="+panel.gameObject.name);
                break;                
            }
        }
        else
        {
            Debug.Log("Item RoomManager'ı bulamadi...");
        }

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            dragRoot.GetComponent<RectTransform>(),
            eventData.position,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out localPoint
        );

        rectTransform.localPosition = localPoint;
        transform.SetParent(dragRoot, false);
        transform.SetAsLastSibling();

        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, originalSizeDelta.x * 1.1f);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, originalSizeDelta.y * 1.1f);
        transform.localScale = originalScale;

        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        transform.SetParent(dragRoot, false);
        transform.localScale = originalScale;

        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, originalSizeDelta.x);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, originalSizeDelta.y);

    }
}