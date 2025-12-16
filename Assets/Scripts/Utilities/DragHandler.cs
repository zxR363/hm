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

    private Vector2 startPosition;

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



    public bool IsValidPlacement { get; private set; } = true; // Default true to avoid accidental destruction on start

    private Vector3? _lastValidLocalPosition;
    private Vector2 dragOffset;
    private RectTransform parentRect;

    void Start()
    {
         // Validate initial placement
         CheckPlacement();
         
         // If valid on start, remember this spot.
         if (IsValidPlacement)
         {
             _lastValidLocalPosition = transform.localPosition;
         }
    }

    public void ForceValidation()
    {
        CheckPlacement();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Refresh References to ensure we use the correct Canvas (ScaleFactor) after reparenting
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        parentRect = rectTransform.parent as RectTransform; // Cache parent rect

        if (canvas == null)
        {
             Debug.LogWarning("[DragHandler] Canvas not found in OnBeginDrag!");
             return; // Safely exit if no canvas
        }

        // Force validation to ensure IsValidPlacement is fresh (checks current spot)
        CheckPlacement();

        // Capture last VALID position before we start moving.
        // This ensures that if we abort/revert, we go back to where we picked it up.
        // But only if current spot is valid! 
        // If we pick up an invalid item, we don't want to save that dirty spot. 
        // We rely on the older valid position (or null).
        if (IsValidPlacement)
        {
             _lastValidLocalPosition = transform.localPosition;
        }

        // Check if click is inside item
        if (!RectTransformUtility.RectangleContainsScreenPoint(rectTransform, eventData.position, eventData.pressEventCamera))
        {
            eventData.pointerDrag = null;
            return;
        }

        // Calculate Offset: Local Mouse Pos - Current LOCAL Pos
        // We use localPosition instead of anchoredPosition because ScreenPointToLocalPointInRectangle
        // gives us the point in the Parent's local space, which directly corresponds to transformed localPosition.
        Vector2 localMousePos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, eventData.position, eventData.pressEventCamera, out localMousePos))
        {
            dragOffset = (Vector2)rectTransform.localPosition - localMousePos;
        }
        else
        {
            dragOffset = Vector2.zero;
        }

        canvasGroup.blocksRaycasts = false;
        
        // Show Garbage Bin
        if (GarbageBinController.Instance != null)
        {
            GarbageBinController.Instance.Show();
            _wasHoveringBin = false;
        }
    }

    private bool _wasHoveringBin = false;

    public void OnDrag(PointerEventData eventData)
    {
        if (parentRect == null) return;

        // Move to: Local Mouse Pos + Offset
        Vector2 localMousePos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, eventData.position, eventData.pressEventCamera, out localMousePos))
        {
            rectTransform.localPosition = localMousePos + dragOffset;
        }

        // Clamp to Parent Bounds
        if (clampToParent)
        {
            ClampToParent();
        }

        CheckPlacement();
        
        // Garbage Bin Hover Logic
        if (GarbageBinController.Instance != null)
        {
            bool isOverBin = GarbageBinController.Instance.IsPointerOverBin(eventData.position);
            if (isOverBin && !_wasHoveringBin)
            {
                GarbageBinController.Instance.OnHoverEnter();
                _wasHoveringBin = true;
            }
            else if (!isOverBin && _wasHoveringBin)
            {
                GarbageBinController.Instance.OnHoverExit();
                _wasHoveringBin = false;
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        
        // Check for Garbage Bin Drop
        if (GarbageBinController.Instance != null)
        {
            if (GarbageBinController.Instance.IsPointerOverBin(eventData.position))
            {
                Debug.Log($"[DragHandler] {name} Dropped on Garbage Bin. Destroying...");
                GarbageBinController.Instance.Hide();
                Destroy(gameObject);
                return; // Exit fast
            }
            GarbageBinController.Instance.Hide();
        }
        
        CheckPlacement();

        if (IsValidPlacement)
        {
            // USER REQUEST: Update "startPoint" so if we revert later (e.g. Everyone Back Home),
            // we revert to THIS valid spot, not the ancient original spot.
            UpdateCurrentPositionAsValid();
            startPosition = rectTransform.anchoredPosition; 
        }
        else
        {
            TryResetPosition();
        }
    }

    /// <summary>
    /// Attempts to revert the item to its last known valid position.
    /// Returns TRUE if reverted successfully.
    /// Returns FALSE if no valid history exists (should probably be destroyed).
    /// </summary>
    public bool TryResetPosition()
    {
        if (_lastValidLocalPosition.HasValue)
        {
            transform.localPosition = _lastValidLocalPosition.Value;
            CheckPlacement(); // Re-validate to update visuals (Red -> White)
            // Debug.Log($"[DragHandler] {name} Reverting to SAVED Valid Position: {_lastValidLocalPosition.Value}");
            return true;
        }
        
        Debug.LogWarning("[DragHandler] No valid history to revert to!");
        return false;
    }

    /// <summary>
    /// Forces the DragHandler to adopt the current local position as the new valid baseline.
    /// Call this after reparenting or programmatic movement.
    /// </summary>
    public void UpdateCurrentPositionAsValid()
    {
        _lastValidLocalPosition = transform.localPosition;
        startPosition = rectTransform.anchoredPosition; // Update startPosition for "Back Home" logic
        // Debug.Log($"[DragHandler] {name} Saved NEW Valid Position: {_lastValidLocalPosition}");
    }

    private void CheckPlacement()
    {
        // USER REQUEST: Only objects with IItemBehaviours should exhibit this collision logic.
        if (GetComponent<IItemBehaviours>() == null)
        {
            IsValidPlacement = true;
            return;
        }

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

        bool collisionDetected = false;
        bool areaFound = false;

        if (count > 0)
        {
            foreach (Collider2D hitCollider in results)
            {
                // ... (Existing Ignores)
                GameObject hitObj = hitCollider.gameObject;
                if (hitObj == gameObject) continue;
                if (hitObj.transform.IsChildOf(transform)) continue;

                CanvasGroup hitCG = hitObj.GetComponent<CanvasGroup>();
                if (hitCG == null) hitCG = hitObj.GetComponentInParent<CanvasGroup>();
                if (hitCG != null && !hitCG.blocksRaycasts) continue;

                // 1. Placement Check (Collision)
                ItemPlacement otherPlacement = hitObj.GetComponent<ItemPlacement>();
                if (otherPlacement != null)
                {
                    if (_itemPlacement.allowedType == PlacementType.All || otherPlacement.allowedType == PlacementType.All) { /* Allowed */ }
                    else
                    {
                        collisionDetected = true;
                        // Debug.Log($"[DragHandler] {name} Invalid: Collided with {hitObj.name}");
                        break; 
                    }
                }

                // 2. Area Check
                PlacementArea area = hitObj.GetComponent<PlacementArea>();
                if (area == null) area = hitObj.GetComponentInParent<PlacementArea>();
                
                if (area != null)
                {
                    if (_itemPlacement.allowedType == PlacementType.Both || _itemPlacement.allowedType == area.type)
                    {
                        areaFound = true;
                    }
                }
            }
        }
        
        IsValidPlacement = !collisionDetected && areaFound;

        if (!IsValidPlacement)
        {
             if (collisionDetected) { /* Logged above */ }
             else if (!areaFound) Debug.Log($"[DragHandler] {name} Invalid: No Valid Placement Area Found! (Count={count})");
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