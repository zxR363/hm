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

            // --- SCREEN BOUNDS CLAMPING ---
            // Calculate Screen Safe Area in World Space
            Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            if (cam == null && canvas.renderMode != RenderMode.ScreenSpaceOverlay) cam = Camera.main;

            Vector3 screenMin, screenMax;

            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                Rect safeArea = Screen.safeArea;
                screenMin = new Vector3(safeArea.xMin, safeArea.yMin, -float.MaxValue);
                screenMax = new Vector3(safeArea.xMax, safeArea.yMax, float.MaxValue);
            }
            else
            {
                // For Camera modes, convert Safe Area to World Space
                Rect safeArea = Screen.safeArea;
                // Use the object's depth for accurate conversion
                float zDepth = transform.position.z - cam.transform.position.z;
                screenMin = cam.ScreenToWorldPoint(new Vector3(safeArea.xMin, safeArea.yMin, zDepth));
                screenMax = cam.ScreenToWorldPoint(new Vector3(safeArea.xMax, safeArea.yMax, zDepth));
            }

            // Use Screen Bounds directly
            Vector3 finalMin = screenMin;
            Vector3 finalMax = screenMax;

            // Calculate Shift needed to keep Bounds inside Screen
            Vector3 shift = Vector3.zero;

            // X Axis
            if (bounds.min.x < finalMin.x)
                shift.x = finalMin.x - bounds.min.x;
            else if (bounds.max.x > finalMax.x)
                shift.x = finalMax.x - bounds.max.x;

            // Y Axis
            if (bounds.min.y < finalMin.y)
                shift.y = finalMin.y - bounds.min.y;
            else if (bounds.max.y > finalMax.y)
                shift.y = finalMax.y - bounds.max.y;

            // Apply Shift
            if (shift != Vector3.zero)
            {
                transform.position += shift;
            }
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