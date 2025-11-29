using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ContentScrollHelper : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private ScrollRect scrollRect;

    private void Awake()
    {
        scrollRect = GetComponentInParent<ScrollRect>();
        if (scrollRect == null)
        {
            Debug.LogWarning($"[ContentScrollHelper] {name}: ScrollRect parent bulunamadı!");
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (scrollRect != null)
        {
            scrollRect.OnBeginDrag(eventData);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (scrollRect != null)
        {
            scrollRect.OnDrag(eventData);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (scrollRect != null)
        {
            scrollRect.OnEndDrag(eventData);
        }
    }
}
