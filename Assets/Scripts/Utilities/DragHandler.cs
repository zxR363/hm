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
            
            Vector3 parentMin = parentCorners[0];
            Vector3 parentMax = parentCorners[0];
            for (int i = 1; i < 4; i++)
            {
                parentMin = Vector3.Min(parentMin, parentCorners[i]);
                parentMax = Vector3.Max(parentMax, parentCorners[i]);
            }

            // --- SCREEN BOUNDS CLAMPING (ADDITION) ---
            // Calculate Screen Safe Area in World Space
            Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            if (cam == null && canvas.renderMode != RenderMode.ScreenSpaceOverlay) cam = Camera.main;

            Vector3 screenMin, screenMax;

            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                // For Overlay, World Space is roughly Screen Space but scaled? 
                // Actually, GetWorldCorners on UI elements in Overlay mode returns Screen Space coordinates.
                // So we can compare directly with Screen.safeArea.
                Rect safeArea = Screen.safeArea;
                screenMin = new Vector3(safeArea.xMin, safeArea.yMin, -float.MaxValue);
                screenMax = new Vector3(safeArea.xMax, safeArea.yMax, float.MaxValue);
            }
            else
            {
                // For Camera modes, convert Safe Area to World Space
                Rect safeArea = Screen.safeArea;
                Vector3 minScreen = new Vector3(safeArea.xMin, safeArea.yMin, cam.nearClipPlane);
                Vector3 maxScreen = new Vector3(safeArea.xMax, safeArea.yMax, cam.nearClipPlane);
                
                // We need a depth for WorldToWorldPoint? No, ScreenToWorldPoint.
                // Assuming UI is on a plane, we need the corners at the UI depth.
                // But simpler: just intersect the World Bounds of the Parent with the Frustum?
                // Or just use Viewport 0-1.
                
                // Let's use Viewport 0,0 and 1,1 converted to World Point at the object's depth
                float zDepth = transform.position.z - cam.transform.position.z;
                screenMin = cam.ViewportToWorldPoint(new Vector3(0, 0, zDepth));
                screenMax = cam.ViewportToWorldPoint(new Vector3(1, 1, zDepth));
                
                // Safe Area adjustment for Viewport (approximate if needed, or use ScreenToWorldPoint with safeArea pixels)
                screenMin = cam.ScreenToWorldPoint(new Vector3(safeArea.xMin, safeArea.yMin, zDepth));
                screenMax = cam.ScreenToWorldPoint(new Vector3(safeArea.xMax, safeArea.yMax, zDepth));
            }

            // Intersect Parent Bounds with Screen Bounds
            Vector3 finalMin = Vector3.Max(parentMin, screenMin);
            Vector3 finalMax = Vector3.Min(parentMax, screenMax);

            // Calculate required clamp in World Space
            Vector3 currentPos = transform.position;
            Vector3 clampedPos = currentPos;

            // Bounds.min = Center - Extents
            // We want: Bounds.min >= FinalMin  =>  (Pos + Offset - Extents) >= FinalMin
            // We want: Bounds.max <= FinalMax  =>  (Pos + Offset + Extents) <= FinalMax
            
            // Calculate Offset from Transform Position to Bounds Center
            Vector3 offset = bounds.center - currentPos;
            Vector3 extents = bounds.extents;

            // Min Limit: Pos >= FinalMin - Offset + Extents
            Vector3 minLimit = finalMin - offset + extents;
            
            // Max Limit: Pos <= FinalMax - Offset - Extents
            Vector3 maxLimit = finalMax - offset - extents;

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