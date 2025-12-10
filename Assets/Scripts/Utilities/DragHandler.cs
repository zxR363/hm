using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class DragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    [Header("Settings")]
    [SerializeField] private float dragSpeed = 1f;
    [SerializeField] private bool clampToParent = true;

    private ItemPlacement _itemPlacement;
    private UIStickerEffect[] _stickerEffects; // Changed to array
    private Collider2D _dragCollider;
    
    [SerializeField] private Color validColor = Color.white;
    [SerializeField] private Color invalidColor = Color.red;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        _itemPlacement = GetComponent<ItemPlacement>();
        // Find all sticker effects in children as well
        _stickerEffects = GetComponentsInChildren<UIStickerEffect>(true);
        _dragCollider = GetComponentInChildren<Collider2D>(); // Changed to include children

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

    public bool IsValidPlacement { get; private set; } = true; // Default true to avoid accidental destruction on start

    void Start()
    {
         // Validate initial placement
         CheckPlacement();
    }

    public void ForceValidation()
    {
        CheckPlacement();
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

        CheckPlacement();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        // Destruction logic moved to SlidePanelItemButton
    }
    private void CheckPlacement()
    {
        if (_itemPlacement == null)
        {
            Debug.LogWarning($"[DragHandler] No ItemPlacement component found on {name}");
            return;
        }

        if (_dragCollider == null)
        {
             // Try to get it again if it was added dynamically or missed
             _dragCollider = GetComponent<Collider2D>();
             if (_dragCollider == null)
             {
                 _dragCollider = GetComponentInChildren<Collider2D>();
             }
             
             if (_dragCollider == null)
             {
                 Debug.LogWarning($"[DragHandler] No Collider2D found on {name} or children for placement check!");
                 return;
             }
        }

        IsValidPlacement = false;
        
        // Use Collider Overlap instead of UI Raycast
        List<Collider2D> results = new List<Collider2D>();
        ContactFilter2D filter = new ContactFilter2D();
        filter.NoFilter(); // Check everything
        
        int count = _dragCollider.OverlapCollider(filter, results);

        if (count > 0)
        {
            foreach (Collider2D hitCollider in results)
            {
                GameObject hitObj = hitCollider.gameObject;
                // Skip self
                if (hitObj == gameObject) continue;

                PlacementArea area = hitObj.GetComponent<PlacementArea>();
                if (area == null)
                {
                    area = hitObj.GetComponentInParent<PlacementArea>();
                }

                if (area != null)
                {
                    // Debug.Log($"[DragHandler] Collider Hit PlacementArea: {area.name} (Type: {area.type})");
                    
                    if (_itemPlacement.allowedType == PlacementType.Both || 
                        _itemPlacement.allowedType == area.type)
                    {
                        IsValidPlacement = true;
                    }
                    
                    // If we found a valid placement, we can stop searching. 
                    // However, if we found an INVALID one but we might still be touching a VALID one (overlap), 
                    // we should probably keep looking until we find a valid one or run out.
                    if (IsValidPlacement) break; 
                }
            }
        }

        if (_stickerEffects != null)
        {
            foreach (var effect in _stickerEffects)
            {
               if(effect != null)
               {
                   effect.SetOutlineColor(IsValidPlacement ? validColor : invalidColor);
                   effect.enabled = true;
               }
            }
        }
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