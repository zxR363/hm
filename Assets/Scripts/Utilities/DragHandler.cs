using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class DragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    [Header("Settings")]
    [SerializeField] private float dragSpeed = 1f;
    [SerializeField] private bool clampToParent = true;

    [Header("Depth Sorting (Collision)- Derinlik Sıralama")]
    [Tooltip("If true, this item will adjust its Canvas SortingOrder based on Y-position collisions.")]
    [SerializeField] private bool enableDepthSorting = true;
    
    [Header("Bos olmaması gerekiyor derinliğe göre hangi canvas ayarlanacak")]
    [Tooltip("The Canvas to adjust. If null, uses the one on this object.")]
    [SerializeField] private Canvas explicitDepthCanvas; 

    [Tooltip("Sorting Order for the object BEHIND (Higher Y).RoomObject Default")]
    [SerializeField] private int sortingOrderBack = 20;

    [Tooltip("Sorting Order for the object IN FRONT (Lower Y).RoomObject Default +1")]
    [SerializeField] private int sortingOrderFront = 21;

    [Tooltip("Sorting Order when NOT dragging (Idle). Default 20.")]
    [SerializeField] private int restingSortingOrder = 20;

    public Canvas GetDepthCanvas() => explicitDepthCanvas != null ? explicitDepthCanvas : canvas;
    
    // Cache for child canvases to support recursive sorting
    private Canvas[] _childCanvases;

    private ItemPlacement _itemPlacement;
    private UIStickerEffect[] _stickerEffects; // Changed to array
    private Collider2D _dragCollider;
    
    [Header("Outline (Sticker Effect) renkleri")]
    [SerializeField] private Color validColor = Color.white;
    [SerializeField] private Color invalidColor = Color.red;

    private Image image;
    private Collider2D _myCollider; // Cache collider
    private ContactFilter2D _contactFilter;
    private List<Collider2D> _overlappedColliders = new List<Collider2D>();

    private Vector2 startPosition;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        image = GetComponent<Image>();
        _myCollider = GetComponent<Collider2D>();
        
        // Setup filter for triggers/colliders
        _contactFilter.useTriggers = true;
        _contactFilter.useLayerMask = false; // Check all layers or specify if needed
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

        // FIX: Stop parent ScrollRect (if any) to prevent fighting for input
        ScrollRect parentScroll = GetComponentInParent<ScrollRect>();
        if (parentScroll != null)
        {
            parentScroll.StopMovement();
            parentScroll.OnEndDrag(eventData); // Forcefully end scroll drag
        }

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



        // Cache child canvases for recursive sorting
        // We do this OnBeginDrag to ensure we catch any enabled/disabled changes
        Canvas rootCanvas = GetDepthCanvas();
        if (rootCanvas != null)
        {
             _childCanvases = rootCanvas.GetComponentsInChildren<Canvas>(true);
        }

        canvasGroup.blocksRaycasts = false;
        
        // Show Garbage Bin
        if (GarbageBinController.Instance != null)
        {
            GarbageBinController.Instance.Show();
            _wasHoveringBin = false;
        }

        _isDragging = true;
        _lastPointerData = eventData;
    }

    private bool _wasHoveringBin = false;

    private bool _isDragging = false;
    private PointerEventData _lastPointerData;

    public void OnDrag(PointerEventData eventData)
    {
        // Just cache data and trigger side effects (AutoScroll, Bin, etc.)
        _lastPointerData = eventData;
        
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

        // Auto-Scroll Logic (Keep this here to trigger wakeup)
        if (DragAutoScroller.Instance != null)
        {
            DragAutoScroller.Instance.ProcessDrag(eventData.position);
        }
    }

    private void LateUpdate()
    {
        // FRAME-PERFECT DRAG SYNC
        // We update position in LateUpdate to ensure we are pinned to the mouse
        // AFTER any Auto-Scrolling has moved our parent. This prevents "Drift/Jitter".
        if (_isDragging && parentRect != null)
        {
             UpdateDragPosition();
             
             // Clamp and Collision Check every frame
             if (clampToParent) ClampToParent();
             CheckPlacement();
             CheckDepthCollision();
        }
    }

    private void UpdateDragPosition()
    {
        if (parentRect == null) return;
        
        // Use Input.mousePosition directly for smoothest frame-rate independent tracking
        // Or if using Touch, we might need to cache the pointer ID. 
        // For simplicity assuming Mouse/Single Touch or using cached event camera.
        Vector2 screenPos = Input.mousePosition; 
        
        // If we have cached event data (for camera etc), use it? 
        // Input.mousePosition is usually fine for Screen Space Overlay/Camera.
        // But for "PressEventCamera", we should use what we started with.
        
        Camera cam = _lastPointerData?.pressEventCamera;
        
        Vector2 localMousePos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPos, cam, out localMousePos))
        {
            rectTransform.localPosition = localMousePos + dragOffset;
        }
    }

    private void CheckDepthCollision()
    {
        // 1. Check if Feature is Enabled
        if (!enableDepthSorting || _myCollider == null) return;

        // 1. Check if Feature is Enabled
        if (!enableDepthSorting || _myCollider == null) return;

        // USER FIX: Relaxed check. If explicitDepthCanvas is missing, fall back to 'canvas'.
        Canvas myTargetCanvas = GetDepthCanvas();
        
        if (myTargetCanvas == null)
        {
             // Only log if WE REALLY DON'T HAVE A CANVAS
             Debug.LogWarning($"[DragHandler] {name}: Depth Sorting enabled but NO Canvas found (Explicit or Parent)! Aborting.");
             return;
        }

        // Use OverlapCollider to find neighbors
        // Note: _contactFilter is set to triggers=true, layers=all in Awake
        int count = _myCollider.OverlapCollider(_contactFilter, _overlappedColliders);
        
        for (int i = 0; i < count; i++)
        {
            Collider2D other = _overlappedColliders[i];
            
            // Safety Check
            if (other == null || other.gameObject == gameObject) continue;

            // 2. Identify Target Canvas of the Other Object
            // Try to find DragHandler on other object to respect its explicit canvas setting
            Canvas otherTargetCanvas = null;
            DragHandler otherHandler = other.GetComponent<DragHandler>();
            
            if (otherHandler != null)
            {
                // If it's a draggable item, ask it what canvas to sort
                if (!otherHandler.enableDepthSorting) continue; // Respect its setting? Or force? Let's assume if I collide, I care.
                otherTargetCanvas = otherHandler.GetDepthCanvas();
            }
            else
            {
                // Fallback: Just look for a Canvas on the collided object
                otherTargetCanvas = other.GetComponent<Canvas>();
            }

            // 3. Compare and Sort
            if (otherTargetCanvas != null)
            {
                // Safety Check: Don't sort HIDDEN items (assigned -50 by ItemSelectionPanel)
                if (otherTargetCanvas.sortingOrder == -50) continue;

                // Compare Y (Pixels? World?)
                // Use transform.position.y (Feet logic relies on Pivot being correct)
                float myY = transform.position.y;
                float otherY = other.transform.position.y;
                
                // Logic: Higher Y (Top of Screen) = Behind = Order 100
                //        Lower Y (Bottom of Screen) = Front  = Order 101
                
                if (myY > otherY)
                {
                    // I am higher (Valid color area?), so I go back.
                    // If my canvas isn't already "Back", force it.
                    if (myTargetCanvas.sortingOrder != sortingOrderBack)
                    {
                        // Debug.Log($"[DragHandler] {name} ({myY}) > {other.name} ({otherY}) -> Going BACK ({sortingOrderBack}).");
                        SetRecursiveSortingOrder(myTargetCanvas, sortingOrderBack);
                    }
                    // For the other object (if it's not me), force it forward.
                    if (otherTargetCanvas.sortingOrder != sortingOrderFront)
                    {
                        // Debug.Log($"[DragHandler] Pushing {other.name} FRONT ({sortingOrderFront}).");
                        // Protection: If I am a child of 'other', don't let 'other' force ME to front.
                        SetRecursiveSortingOrder(otherTargetCanvas, sortingOrderFront, ignoreCanvas: myTargetCanvas);
                    }
                }
                else
                {
                    // I am lower, so I go front.
                if (myTargetCanvas.sortingOrder != sortingOrderFront)
                {
                    // Debug.Log($"[DragHandler] {name} ({myY}) <= {other.name} ({otherY}) -> Going FRONT ({sortingOrderFront}).");
                    SetRecursiveSortingOrder(myTargetCanvas, sortingOrderFront);
                }
                if (otherTargetCanvas.sortingOrder != sortingOrderBack)
                {
                     // Debug.Log($"[DragHandler] Pushing {other.name} BACK ({sortingOrderBack}).");
                     // Protection: If I am a child of 'other', don't let 'other' force ME to back (if I wanted to be front.. wait logic handles this)
                     // Actually logic is pairwise. But consistent.
                     SetRecursiveSortingOrder(otherTargetCanvas, sortingOrderBack, ignoreCanvas: myTargetCanvas);
                }
            }
        }
    }
    }


private void SetRecursiveSortingOrder(Canvas root, int targetOrder, Canvas ignoreCanvas = null)
{
    if (root == null) return;
    // Check Root
    if (root == ignoreCanvas) return;

    int currentRootOrder = root.sortingOrder;
    
    // OPTIMIZATION: If already at target, skip expensive children scan of "Other" objects
    // This prevents GetComponentsInChildren from running every frame.
    // We assume if root is correct, children are also correct (from previous set).
    if (currentRootOrder == targetOrder) return;
    
    // Update Root
    root.overrideSorting = true;
    root.sortingOrder = targetOrder;
    
    // Update Children (if cached matches root)
    // Note: If we are updating "Other" object, _childCanvases belongs to US, not them.
    // So we must fetch children dynamically for 'root' if it's not us.
    
    Canvas[] targets = null;
    Canvas myCanvas = GetDepthCanvas();
    
    if (root == myCanvas && _childCanvases != null)
    {
        targets = _childCanvases; // Use Cache
    }
    else
    {
        targets = root.GetComponentsInChildren<Canvas>(true); // Expensive but necessary for correctness
    }
    
    if (targets != null)
    {
        foreach (var c in targets)
        {
            if (c == root) continue; // Already handled root
            if (c == ignoreCanvas) continue; // Don't touch the canvas we're trying to ignore

            // USER REQUEST: Strict Rule: Parent=X -> Child=X+1
            // We force overrideSorting = true for ALL children found.
            c.overrideSorting = true;
            //c.sortingOrder = targetOrder + 1;
            c.sortingOrder = targetOrder;
            // Debug.Log($"[DragHandler] Recursion: Child {c.name} set to {c.sortingOrder} (Parent Target: {targetOrder})");
        }
    }
}        
    public void OnEndDrag(PointerEventData eventData)
    {
        _isDragging = false;
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
        // Restore Default Sorting Order (Recursive)
        // This brings the object back to the "Interaction Layer" (e.g. 50)
        Canvas c = GetDepthCanvas();
        if (c != null)
        {
             SetRecursiveSortingOrder(c, restingSortingOrder);
        }

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