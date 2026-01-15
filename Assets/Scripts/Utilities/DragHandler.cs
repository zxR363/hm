using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class DragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    [Header("Settings")]
    [SerializeField] private float dragSpeed = 1f;
    [SerializeField] private bool clampToParent = true;

    [Header("Depth Sorting (Collision)- Derinlik Sıralama")]
    [Tooltip("If true, this item will adjust its Canvas SortingOrder based on Y-position collisions.")]
    [SerializeField] private bool enableDepthSorting = false;
    
    [Header("Bos olmaması gerekiyor derinliğe göre hangi canvas ayarlanacak")]
    [Header("Kendi Image'i olacak")]
    [Tooltip("The Canvas to adjust. If null, uses the one on this object.")]
    [SerializeField] private Canvas explicitDepthCanvas; 

    [Tooltip("Sorting Order for the object BEHIND (Higher Y).RoomObject Default")]
    [SerializeField] private int sortingOrderBack = 20;

    [Tooltip("Sorting Order for the object IN FRONT (Lower Y).RoomObject Default +1")]
    [SerializeField] private int sortingOrderFront = 21;

    [Tooltip("Sorting Order when NOT dragging (Idle). Default 20.")]
    [SerializeField] private int restingSortingOrder = 20;

    [HideInInspector]
    [SerializeField] private string _uniqueId; // Persistence ID

    // definition moved below
    // public Canvas GetDepthCanvas() => explicitDepthCanvas != null ? explicitDepthCanvas : canvas;
    
    // Cache for child canvases to support recursive sorting
    private Canvas[] _childCanvases;

    private CustomGravity _customGravity;
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
        Debug.Log($"[DEBUG_TRACE] {Time.frameCount} - DragHandler Awake on {gameObject.name}");
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        image = GetComponent<Image>();
        _myCollider = GetComponent<Collider2D>();
        
        // Setup filter for triggers/colliders
        _contactFilter.useTriggers = true;
        _contactFilter.useLayerMask = false; // Check all layers or specify if needed
        _itemPlacement = GetComponent<ItemPlacement>();
        _customGravity = GetComponent<CustomGravity>();
        
        // Find all sticker effects in children as well
        _stickerEffects = GetComponentsInChildren<UIStickerEffect>(true);
        _dragCollider = GetComponentInChildren<Collider2D>(); // Changed to include children

        if (canvasGroup == null)
        {
             Debug.LogWarning($"[DragHandler] {name} missing CanvasGroup. Auto-adding to prevent crash/errors.");
             canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // AUTO-FIX: If we have a Canvas, we MUST have a GraphicRaycaster to receive events!
        if (GetComponent<Canvas>() != null && GetComponent<GraphicRaycaster>() == null)
        {
             Debug.LogWarning($"[DragHandler] {name} has Canvas but NO GraphicRaycaster. Auto-adding to fix Input.");
             gameObject.AddComponent<GraphicRaycaster>();
        }
    }



    public bool IsValidPlacement { get; private set; } = true; // Default true to avoid accidental destruction on start

    private Vector3? _lastValidLocalPosition;
    private Vector2 dragOffset;
    private RectTransform parentRect;
    
    // START STATE TRACKING (Added for Character detach logic)
    private Transform _startParent;
    private Vector3 _startLocalPosition;

    void Start()
    {
         // AUTO-FIX: Ensure we have a Canvas for sorting, otherwise Drag sorting fails.
         if (GetComponent<Canvas>() == null)
         {
             Debug.Log($"[DragHandler] {name} missing Canvas. Auto-adding to support Sorting Order Logic.");
             Canvas c = gameObject.AddComponent<Canvas>();
             c.overrideSorting = true;
             c.sortingOrder = restingSortingOrder;
             
             // Must have Raycaster too
             gameObject.AddComponent<GraphicRaycaster>();
             
             // Update references
             canvas = c;
         }


         // Validate initial placement
         CheckPlacement();
         
         // If valid on start, remember this spot.
         if (IsValidPlacement)
         {
             _lastValidLocalPosition = transform.localPosition;
         }
    }

    /*
    private void OnValidate()
    {
        // AUTO-GENERATE ID (Zero Setup)
        // If ID is empty (or we are duplicating/resetting), generate a new one.
        // Needs proper logic to avoid regenerating on every script reload, but OnValidate runs on load.
        // We only generate if empty.
        #if UNITY_EDITOR
        if (string.IsNullOrEmpty(_uniqueId))
        {
            _uniqueId = System.Guid.NewGuid().ToString();
            UnityEditor.EditorUtility.SetDirty(this);
        }
        #endif
    }
    */

    public void ForceValidation()
    {
        CheckPlacement();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log($"[DragHandler] OnBeginDrag STARTED on {name}.");

        // -------------------------------------------------------------
        // NEW: Check if held by character (Phase 2 Detachment)
        // -------------------------------------------------------------
        var holdable = GetComponent<AvatarWorld.Interaction.HoldableItem>();
        if (holdable != null && holdable.isHeld)
        {
             Debug.Log($"[DragHandler] {name} is HELD. Attempting to detach...");
             
             // Find who is holding us (HandSlot -> Character)
             var handCtrl = GetComponentInParent<AvatarWorld.Interaction.CharacterHandController>();
             if (handCtrl != null)
             {
                  // 1. Notify Controller to release logical grip
                  handCtrl.DropItem(holdable);
                  
                  // NEW ROBUST FIX: Directly become Sibling of the Character (Attach to Character's Parent aka Room)
                  if (handCtrl.transform.parent != null)
                  {
                      _startParent = handCtrl.transform.parent; // Remember where we belong (Room)
                      _startLocalPosition = _startParent.InverseTransformPoint(transform.position);
                      
                      transform.SetParent(_startParent, true); 
                      transform.SetAsLastSibling(); // Ensure we render in front of our sibling (Character)
                      Debug.Log($"[DragHandler] Detach Success: Reparented to {_startParent.name} (Character's Parent).");
                  }
                  else
                  {
                      // Fallback if Character has no parent (Unlikely?)
                      Canvas currentCanvas = GetComponentInParent<Canvas>();
                      if (currentCanvas != null)
                      {
                          transform.SetParent(currentCanvas.transform, true);
                          transform.SetAsLastSibling();
                          Debug.Log($"[DragHandler] Reparented to Canvas ({currentCanvas.name}).");
                      }
                  }
                  Debug.Log($"[DragHandler] Detached {name} from Character.");
             }
             else
             {
                 Debug.LogWarning("[DragHandler] Held item has no CharacterHandController parent?!");
             }
        }
        else if (holdable != null)
        {
             // Debug.Log($"[DragHandler] Detach check skipped. isHeld: {holdable.isHeld}");
        }
        // -------------------------------------------------------------



        // -------------------------------------------------------------
        // NEW: Check if Sitting (Phase 4)
        // -------------------------------------------------------------
        var sitter = GetComponent<AvatarWorld.Interaction.CharacterSittingController>();
        if (sitter != null && sitter.IsSitting)
        {
             Debug.Log($"[DragHandler] {name} was sitting. Standing up...");
             sitter.StandUp();
        }
        
        // -------------------------------------------------------------
        // NEW: Check if Sleeping (Phase 5)
        // -------------------------------------------------------------
        var sleeper = GetComponent<AvatarWorld.Interaction.CharacterSleepingController>();
        if (sleeper != null && sleeper.IsSleeping)
        {
             Debug.Log($"[DragHandler] {name} was sleeping. Waking up...");
             sleeper.WakeUp();
        }
        // -------------------------------------------------------------

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
        
        // BUGFIX: Force high sorting order immediately on Drag Start.
        // This prevents the item from disappearing behind other objects (like Character Face) during drag.
        int dragSortOrder = Mathf.Max(sortingOrderFront, 100); 
        
        if (rootCanvas != null)
        {
             Debug.Log($"[DragHandler] Force Sorting ON. Canvas: {rootCanvas.name}, OldOrder: {rootCanvas.sortingOrder}, NewOrder: {dragSortOrder}, Layer: {rootCanvas.sortingLayerName}");
             SetRecursiveSortingOrder(rootCanvas, dragSortOrder);
        }
        else
        {
             Debug.LogError($"[DragHandler] CRITICAL: No Canvas found on {name}! Cannot Apply Sorting Order 100. Hierarchy depth might fail.");
        }

        _lastPointerData = eventData;
        
        if (_customGravity != null) _customGravity.StopFalling();
    }

    private bool _wasHoveringBin = false;
    private AvatarWorld.Interaction.CharacterEatingController _lastEatingController; // Track last eater for feedback

    private bool _isDragging = false;
    private PointerEventData _lastPointerData;

    public void OnDrag(PointerEventData eventData)
    {
        // Just cache data and trigger side effects (AutoScroll, Bin, etc.)
        _lastPointerData = eventData;
        
        // -------------------------------------------------------------
        // NEW: Eating Feedback (Mouth Open)
        // -------------------------------------------------------------
        var consumable = GetComponent<AvatarWorld.Interaction.ConsumableItem>();
        if (consumable != null)
        {
             AvatarWorld.Interaction.CharacterEatingController currentEater = null;
             
             // Check if over any character
             var allEaters = FindObjectsOfType<AvatarWorld.Interaction.CharacterEatingController>();
             foreach (var eater in allEaters)
             {
                 // USE SPECIFIC RECT IF AVAILABLE, OTHERWISE FALLBACK TO TRANSFORM
                 RectTransform targetRect = eater.mouthDetectionRect != null ? eater.mouthDetectionRect : (eater.transform as RectTransform);
                 
                 if (targetRect == null) continue;

                 Canvas eaterCanvas = eater.GetComponentInParent<Canvas>();
                 Camera camToUse = Camera.main;
                 if (eaterCanvas != null && eaterCanvas.renderMode == RenderMode.ScreenSpaceOverlay) camToUse = null;

                 if (RectTransformUtility.RectangleContainsScreenPoint(targetRect, eventData.position, camToUse))
                 {
                      currentEater = eater;
                      break; // Found one
                 }
             }

             // Logic for Enter/Exit
             if (currentEater != _lastEatingController)
             {
                 // We switched targets
                 // 1. Close old mouth
                 if (_lastEatingController != null) _lastEatingController.OnFoodNearby(false);

                 // 2. Open new mouth
                 if (currentEater != null) currentEater.OnFoodNearby(true);

                 _lastEatingController = currentEater;
             }
        }
        else if (_lastEatingController != null)
        {
            // If we somehow lost the consumable component mid-drag? or (more likely) copied logic to non-consumable
            _lastEatingController.OnFoodNearby(false);
            _lastEatingController = null;
        }
        // -------------------------------------------------------------

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

        // Auto-Scroll Logic: Use RectTransform for Bounds-Aware Scrolling
        if (DragAutoScroller.Instance != null)
        {
            DragAutoScroller.Instance.ProcessDrag(rectTransform);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log($"[DragHandler] OnPointerDown on {name} (Raycast Hit!). Interactions are WORKING.");
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

    // FIX: Only use the 'canvas' found in Awake if it is THIS object's canvas.
    // If we use 'canvas' (which is GetComponentInParent), we might sort the Room's Root Canvas
    // which messes up the entire hierarchy.
    public Canvas GetDepthCanvas() 
    {
        if (explicitDepthCanvas != null) return explicitDepthCanvas;
        
        // Try to find a canvas strictly on THIS object or immediate children?
        // Actually, just checking if 'canvas' is on this gameObject is enough safety?
        // Awake: canvas = GetComponentInParent<Canvas>(); -> This could be parent.
        
        Canvas localCanvas = GetComponent<Canvas>();
        if (localCanvas != null) return localCanvas;

        return null; // Do NOT fall back to shared parent canvas.
    }
    
    // ...

    private void CheckDepthCollision()
    {
        // 1. Check if Feature is Enabled
        if (!enableDepthSorting || _myCollider == null) return;

        // 1. Check if Feature is Enabled
        if (_isDragging) return; // FIX: Don't override sorting while dragging!
        
        if (!enableDepthSorting || _myCollider == null) return;

        // USER FIX: Relaxed check. If explicitDepthCanvas is missing, fall back to 'canvas'.
        Canvas myTargetCanvas = GetDepthCanvas();
        
        if (myTargetCanvas == null)
        {
             // We can't sort if we don't have our own canvas.
             // We decided NOT to sort the shared parent canvas to avoid chaos.
             // Return silently or with debug log if debugging.
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

            // BUGFIX: Do NOT override sorting of items held by this character (or attached to it)
            // They manage their own sorting (e.g. attached to hand = Order 30).
            if (c.GetComponent<AvatarWorld.Interaction.HoldableItem>() != null) continue;

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
        _isDragging = false;
        canvasGroup.blocksRaycasts = true;
        
        if (_customGravity != null) _customGravity.StartFalling();

        // -------------------------------------------------------------
        // NEW: Check for Eating (Phase 6) - PRIORITIZED!
        // -------------------------------------------------------------
        var consumable = GetComponent<AvatarWorld.Interaction.ConsumableItem>();
        if (consumable != null)
        {
             // Check if dropped on Character's Mouth Area
             var allEaters = FindObjectsOfType<AvatarWorld.Interaction.CharacterEatingController>();
             foreach (var eater in allEaters)
             {
                 // USE SPECIFIC RECT IF AVAILABLE, OTHERWISE FALLBACK TO TRANSFORM
                 RectTransform targetRect = eater.mouthDetectionRect != null ? eater.mouthDetectionRect : (eater.transform as RectTransform);
                 
                 if (targetRect == null) continue;

                 // Handle Canvas Render Mode
                 Canvas eaterCanvas = eater.GetComponentInParent<Canvas>();
                 Camera camToUse = Camera.main;
                 if (eaterCanvas != null && eaterCanvas.renderMode == RenderMode.ScreenSpaceOverlay) camToUse = null;

                 if (RectTransformUtility.RectangleContainsScreenPoint(targetRect, eventData.position, camToUse))
                 {
                      Debug.Log($"[DragHandler] Fed {name} to {eater.name}.");
                      eater.OnFoodNearby(false); // Close mouth before eating
                      eater.Eat(consumable);
                      return; // Consumed (or at least logic handled)
                 }
             }
        }
        // -------------------------------------------------------------

        // -------------------------------------------------------------
        // NEW: Check for Holdable Item Drop (Phase 2)
        // -------------------------------------------------------------
        Debug.Log("[DragHandler] OnEndDrag STARTED."); // ENTRY LOG
        
        var holdable = GetComponent<AvatarWorld.Interaction.HoldableItem>();
        if (holdable != null)
        {
            Debug.Log($"[DragHandler] [UI Math Check] Checking drop for {name}...");

            // FIX: Using RectTransformUtility. Detects if mouse is inside ANY Character's Rect.
            // Works for Overlay, Camera, World Space - ANY Canvas mode.
            // Does NOT require Colliders. Does NOT require RaycastTargets.
            
            var allCharacters = FindObjectsOfType<AvatarWorld.Interaction.CharacterHandController>();
            
            foreach (var charCtrl in allCharacters)
            {
                // Logic: Check 'HandDetectionRects' first. If empty, fallback to 'transform'?? 
                // USER REQUEST: Only specific areas. So if array is empty, maybe we should NOT hold?
                // Let's assume fallback to 'transform' for backward compatibility if user hasn't assigned rects yet.
                
                // Collect Rects to check
                System.Collections.Generic.List<RectTransform> rectsToCheck = new System.Collections.Generic.List<RectTransform>();
                
                if (charCtrl.handDetectionRects != null && charCtrl.handDetectionRects.Length > 0)
                {
                    rectsToCheck.AddRange(charCtrl.handDetectionRects);
                }
                else
                {
                    // Fallback
                    rectsToCheck.Add(charCtrl.transform as RectTransform);
                }

                foreach(var targetRect in rectsToCheck)
                {
                    if (targetRect == null) continue;

                    // Handle Canvas Render Mode for correct Math
                    Canvas charCanvas = charCtrl.GetComponentInParent<Canvas>();
                    Camera camToUse = Camera.main;
    
                    if (charCanvas != null && charCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
                    {
                         camToUse = null; // Overlay requires null!
                    }
    
                    // Check if Mouse Position is inside the specific target rect
                    if (RectTransformUtility.RectangleContainsScreenPoint(targetRect, eventData.position, camToUse))
                    {
                         if (charCtrl.TryHoldItem(holdable))
                         {
                             // Successfully grabbed!
                             return;
                         }
                    }
                }
            }
        }
        // -------------------------------------------------------------
        // -------------------------------------------------------------
        
        // -------------------------------------------------------------
        // NEW: Check for Sitting Spot Drop (Phase 4)
        // -------------------------------------------------------------
        var characterSitter = GetComponent<AvatarWorld.Interaction.CharacterSittingController>();
        if (characterSitter != null)
        {
             // Find all Seats
             var allSeats = FindObjectsOfType<AvatarWorld.House.Furniture.Seat>();
             foreach (var seat in allSeats)
             {
                 RectTransform seatRect = seat.transform as RectTransform;
                 if (seatRect == null) continue;

                 // Handle Canvas Render Mode
                 Canvas seatCanvas = seat.GetComponentInParent<Canvas>();
                 Camera camToUse = Camera.main;
                 if (seatCanvas != null && seatCanvas.renderMode == RenderMode.ScreenSpaceOverlay) camToUse = null;

                 if (RectTransformUtility.RectangleContainsScreenPoint(seatRect, eventData.position, camToUse))
                 {
                      Debug.Log($"[DragHandler] Dropped Character on Seat: {seat.name}");
                      characterSitter.TrySit(seat);
                      
                      // Also ensure we are parented to the Room/Area of the seat for correct depth?
                      // Usually Seats are in Rooms.
                      RoomPanel room = seat.GetComponentInParent<RoomPanel>();
                      if (room != null)
                      {
                           Transform targetParent = room.objectContainer != null ? room.objectContainer : room.transform;
                           transform.SetParent(targetParent, true);
                      }
                      return; // Handle sitting and exit
                 }
             }
        }
            
        // -------------------------------------------------------------
        // NEW: Check for Bed Drop (Phase 5)
        // -------------------------------------------------------------
        var characterSleeper = GetComponent<AvatarWorld.Interaction.CharacterSleepingController>();
        if (characterSleeper != null)
        {
             var allBeds = FindObjectsOfType<AvatarWorld.House.Furniture.Bed>();
             foreach (var bed in allBeds)
             {
                 RectTransform bedRect = bed.transform as RectTransform;
                 if (bedRect == null) continue;

                 // Handle Canvas Render Mode
                 Canvas bedCanvas = bed.GetComponentInParent<Canvas>();
                 Camera camToUse = Camera.main;
                 if (bedCanvas != null && bedCanvas.renderMode == RenderMode.ScreenSpaceOverlay) camToUse = null;

                 if (RectTransformUtility.RectangleContainsScreenPoint(bedRect, eventData.position, camToUse))
                 {
                      Debug.Log($"[DragHandler] Dropped Character on Bed: {bed.name}");
                      characterSleeper.TrySleep(bed);
                      
                      // Reparent to Room
                      RoomPanel room = bed.GetComponentInParent<RoomPanel>();
                      if (room != null)
                      {
                           Transform targetParent = room.objectContainer != null ? room.objectContainer : room.transform;
                           transform.SetParent(targetParent, true);
                      }
                      return;
                 }
             }
        }

        // ------------------------------------------------------------- 
        


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
        // USER REQUEST: If valid drop AND has CustomGravity (Physics Item), 
        // ensure it belongs to the Room Panel (detach from any stack).
        // This prevents items from staying children of other items after being moved away.
        // USER REQUEST: If valid drop, 
        // ensure it belongs to the Room Panel (detach from any stack).
        // This prevents items from staying children of other items after being moved away.
        // REMOVED CustomGravity check: Applies to ALL items now.
        IRoomPanel room = FindRoomPanelUnderMouse();
        if (room != null)
        {
             Transform targetContainer = room.objectContainer != null ? room.objectContainer : room.transform;
             if (transform.parent != targetContainer)
             {
                 transform.SetParent(targetContainer, true);
             }
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
    
    // BUGFIX: ALWAYS restore default sorting order when drag ends, regardless of placement validity.
    // If invalid, we reverted position. If valid, we stayed. In both cases, we are no longer "Highest Priority".
    Canvas c = GetDepthCanvas();
    if (c != null)
    {
         SetRecursiveSortingOrder(c, restingSortingOrder);
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

        // NEW: Get UNION of Collider Bounds and Visual Bounds (RectTransforms)
        // This ensures that even if the Collider is small, we clamp the larger Visual Image.
        Bounds combinedBounds = new Bounds(transform.position, Vector3.zero);
        bool hasBounds = false;

        // 1. Collider Bounds
        Bounds? colliderBounds = GetCompoundColliderBounds(transform);
        if (colliderBounds.HasValue)
        {
            combinedBounds = colliderBounds.Value;
            hasBounds = true;
        }

        // 2. Visual Bounds (RectTransforms including children)
        // We accumulate World Corners of all child Rects
        var rects = GetComponentsInChildren<RectTransform>();
        Vector3[] corners = new Vector3[4];
        
        foreach (var rt in rects)
        {
            rt.GetWorldCorners(corners);
            if (!hasBounds)
            {
                // Initialize with first corner
                combinedBounds = new Bounds(corners[0], Vector3.zero);
                hasBounds = true;
                // Encapsulate rest
                for(int k=1; k<4; k++) combinedBounds.Encapsulate(corners[k]);
            }
            else
            {
                for(int k=0; k<4; k++) combinedBounds.Encapsulate(corners[k]);
            }
        }

        if (hasBounds)
        {
            // --- SCREEN BOUNDS CLAMPING ---
            // Calculate Screen Safe Area in World Space
            Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            if (cam == null && canvas.renderMode != RenderMode.ScreenSpaceOverlay) cam = Camera.main;

            // USER REQUEST: Allow 15 pixels of "Bleed" (Tolerance) outside the screen
            // We expand the Screen/SafeArea Rect by 15 pixels on all sides
            float pixelBuffer = 30f;
            Rect expandedRect = Screen.safeArea;
            expandedRect.xMin -= pixelBuffer;
            expandedRect.yMin -= pixelBuffer;
            expandedRect.xMax += pixelBuffer;
            expandedRect.yMax += pixelBuffer;

            Vector3 screenMin, screenMax;

            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                screenMin = new Vector3(expandedRect.xMin, expandedRect.yMin, -float.MaxValue);
                screenMax = new Vector3(expandedRect.xMax, expandedRect.yMax, float.MaxValue);
            }
            else
            {
                // For Camera modes, convert Safe Area to World Space
                // Use the object's depth for accurate conversion
                float zDepth = transform.position.z - cam.transform.position.z;
                screenMin = cam.ScreenToWorldPoint(new Vector3(expandedRect.xMin, expandedRect.yMin, zDepth));
                screenMax = cam.ScreenToWorldPoint(new Vector3(expandedRect.xMax, expandedRect.yMax, zDepth));
            }

            // Use Screen Bounds directly
            Vector3 finalMin = screenMin;
            Vector3 finalMax = screenMax;

            // Calculate Shift needed to keep Bounds inside Screen
            Vector3 shift = Vector3.zero;

            // X Axis
            if (combinedBounds.min.x < finalMin.x)
                shift.x = finalMin.x - combinedBounds.min.x;
            else if (combinedBounds.max.x > finalMax.x)
                shift.x = finalMax.x - combinedBounds.max.x;

            // Y Axis
            if (combinedBounds.min.y < finalMin.y)
                shift.y = finalMin.y - combinedBounds.min.y;
            else if (combinedBounds.max.y > finalMax.y)
                shift.y = finalMax.y - combinedBounds.max.y;

            // Apply Shift
            if (shift != Vector3.zero)
            {
                transform.position += shift;
            }
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

    // Helper from ItemDragPanel logic
    private IRoomPanel FindRoomPanelUnderMouse()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (var result in results)
        {
            // Skip self
            if (result.gameObject == gameObject) continue;

            var panel = result.gameObject.GetComponent<IRoomPanel>();
            if (panel == null) panel = result.gameObject.GetComponentInParent<IRoomPanel>();

            if (panel != null && panel.gameObject.activeInHierarchy)
            {
                return panel;
            }
        }
        return null;
    }
}