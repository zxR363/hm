using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;

public class UIDragItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Transform originalParent;
    private Vector3 worldPosBeforeDrag;

    [SerializeField] private Transform dragRoot; // HouseCanvas
    [SerializeField] private Collider2D fridgeArea; // Button_Fridge collider
    [SerializeField] private Transform[] spawnPoints; // spawnPoint_1, spawnPoint_2, ...

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        worldPosBeforeDrag = transform.position;
        originalParent = transform.parent;
        transform.SetParent(dragRoot, false); // scale bozulmaz
        transform.position = worldPosBeforeDrag; // pozisyon korunur
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / dragRoot.GetComponent<Canvas>().scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // Fridge alanı içinde mi?
        bool isInsideFridge = fridgeArea.OverlapPoint(transform.position);

        if (isInsideFridge)
        {
            // En yakın spawnPoint'e yerleştir
            Transform closest = spawnPoints.OrderBy(sp => Vector3.Distance(transform.position, sp.position)).First();
            transform.SetParent(closest, false);
            transform.localPosition = Vector3.zero;
            Debug.Log("Item Fridge içinde bırakıldı → spawnPoint'e yerleşti.");
        }
        else
        {
            // KitchenPanel'e ait oldu
            transform.SetParent(dragRoot, false); // HouseCanvas altında kalır
            Debug.Log("Item Fridge dışına çıktı → KitchenPanel'e ait oldu.");
        }
    }
}
