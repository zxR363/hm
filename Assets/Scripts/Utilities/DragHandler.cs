using UnityEngine;
using UnityEngine.EventSystems;

public class DragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    [Header("Settings")]
    [SerializeField] private float dragSpeed = 1f;
    [SerializeField] private bool clampToParent = true;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Sadece root RectTransform alanı içinde tıklama varsa drag başlasın
        if (!RectTransformUtility.RectangleContainsScreenPoint(rectTransform, eventData.position, eventData.pressEventCamera))
        {
            eventData.pointerDrag = null;
            return;
        }

        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Apply Drag Speed
        rectTransform.anchoredPosition += (eventData.delta * dragSpeed) / canvas.scaleFactor;

        // Clamp to Parent Bounds
        if (clampToParent)
        {
            ClampToParent();
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
    }

    private void ClampToParent()
    {
        RectTransform parentRect = rectTransform.parent as RectTransform;
        if (parentRect == null) return;

        // 1. Try to get Compound Collider Bounds (World Space)
        Bounds? colliderBounds = GetCompoundColliderBounds(transform);

        if (colliderBounds.HasValue)
        {
            // --- COLLIDER BASED CLAMPING ---
            Bounds bounds = colliderBounds.Value;
            
            // Get Parent World Bounds
            Vector3[] parentCorners = new Vector3[4];
            parentRect.GetWorldCorners(parentCorners);
            
            // Calculate Parent Min/Max in World Space
            Vector3 parentMin = parentCorners[0];
            Vector3 parentMax = parentCorners[0];
            for (int i = 1; i < 4; i++)
            {
                parentMin = Vector3.Min(parentMin, parentCorners[i]);
                parentMax = Vector3.Max(parentMax, parentCorners[i]);
            }

            // Calculate required clamp in World Space
            Vector3 currentPos = transform.position;
            Vector3 clampedPos = currentPos;

            // Bounds.min = Center - Extents
            // We want: Bounds.min >= ParentMin  =>  (Pos + Offset - Extents) >= ParentMin
            // We want: Bounds.max <= ParentMax  =>  (Pos + Offset + Extents) <= ParentMax
            
            // Calculate Offset from Transform Position to Bounds Center
            Vector3 offset = bounds.center - currentPos;
            Vector3 extents = bounds.extents;

            // Min Limit: Pos >= ParentMin - Offset + Extents
            Vector3 minLimit = parentMin - offset + extents;
            
            // Max Limit: Pos <= ParentMax - Offset - Extents
            Vector3 maxLimit = parentMax - offset - extents;

            // Clamp
            clampedPos.x = Mathf.Clamp(currentPos.x, minLimit.x, maxLimit.x);
            clampedPos.y = Mathf.Clamp(currentPos.y, minLimit.y, maxLimit.y);

            transform.position = clampedPos;
        }
        else
        {
            // --- FALLBACK: RECT TRANSFORM CLAMPING ---
            Vector3 pos = rectTransform.localPosition;

            Vector3 minPosition = parentRect.rect.min - rectTransform.rect.min;
            Vector3 maxPosition = parentRect.rect.max - rectTransform.rect.max;

            pos.x = Mathf.Clamp(pos.x, minPosition.x, maxPosition.x);
            pos.y = Mathf.Clamp(pos.y, minPosition.y, maxPosition.y);

            rectTransform.localPosition = pos;
        }
    }

    private Bounds? GetCompoundColliderBounds(Transform root)
    {
        Collider2D[] colliders = root.GetComponentsInChildren<Collider2D>();
        if (colliders.Length == 0) return null;

        Bounds bounds = colliders[0].bounds;
        for (int i = 1; i < colliders.Length; i++)
        {
            bounds.Encapsulate(colliders[i].bounds);
        }
        return bounds;
    }
}