using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;

public class ItemDragPanel : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;

    [SerializeField] private Vector3 originalScale;
    [SerializeField] private Vector2 originalSizeDelta;

    [SerializeField] private GameObject itemPrefab; // spawn edilecek yeni item
    public GameObject ItemPrefab => itemPrefab; // Public getter for persistence lookup
    
    // Stores the full Resources path for this item (assigned by AutoLoadTabContents)
    public string ResourcePath; 
    
    private GameObject dragGhost; // geçici görsel kopya
    private Transform dragRoot;   // aktif RoomPanel

    private int defaultDragGHostSortOrder = 0;
    
    // Scroll handling
    private ScrollRect parentScrollRect;
    private bool isScrolling = false;
    private bool isDraggingItem = false;
    private bool isDirectionDecided = false;
    
    [Header("Settings")]
    [SerializeField] private float dragThreshold = 5f; // Pixels to move before deciding (Lower = more sensitive)

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        // Find parent ScrollRect
        parentScrollRect = GetComponentInParent<ScrollRect>();

        // Initialize defaults if not set in Inspector
        if (originalScale == Vector3.zero) originalScale = Vector3.one;
        if (originalSizeDelta == Vector2.zero && rectTransform != null) originalSizeDelta = rectTransform.sizeDelta;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isScrolling = false;
        isDraggingItem = false;
        isDirectionDecided = false;
        
        // Pass to ScrollRect just in case, but we might cancel it later if we decide to drag item
        if (parentScrollRect != null) parentScrollRect.OnBeginDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDirectionDecided)
        {
            Vector2 delta = eventData.position - eventData.pressPosition;
            if (delta.magnitude > dragThreshold)
            {
                isDirectionDecided = true;
                if (Mathf.Abs(delta.y) > Mathf.Abs(delta.x))
                {
                    // Vertical -> Scroll
                    isScrolling = true;
                    isDraggingItem = false;
                }
                else
                {
                    // Horizontal -> Item Drag
                    isScrolling = false;
                    isDraggingItem = true;
                    StartItemDrag();
                }
            }
            else
            {
                if (parentScrollRect != null) parentScrollRect.OnDrag(eventData);
            }
            return;
        }

        if (isScrolling)
        {
            if (parentScrollRect != null) parentScrollRect.OnDrag(eventData);
        }
        else if (isDraggingItem)
        {
            UpdateItemDrag(eventData);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isScrolling)
        {
            if (parentScrollRect != null) parentScrollRect.OnEndDrag(eventData);
        }
        else if (isDraggingItem)
        {
            EndItemDrag();
        }
        
        isScrolling = false;
        isDraggingItem = false;
        isDirectionDecided = false;

    }

    private void StartItemDrag()
    {
        // Find the main canvas to drag on top of everything
        canvas = GetComponentInParent<Canvas>();
        if (canvas != null && canvas.rootCanvas != null)
        {
            canvas = canvas.rootCanvas; // Use root canvas
            dragRoot = canvas.transform;
        }
        else
        {
            Debug.LogWarning("Canvas bulunamadı.");
            isDraggingItem = false;
            return;
        }

        // Geçici görsel kopya oluştur
        dragGhost = Instantiate(itemPrefab, transform.position, Quaternion.identity, dragRoot);

        // PERSISTENCE: Pass the Resource Path to the new object
        RoomObject ghostRoomObj = dragGhost.GetComponent<RoomObject>();
        if (ghostRoomObj == null) ghostRoomObj = dragGhost.AddComponent<RoomObject>();
        ghostRoomObj.loadedFromResourcePath = ResourcePath;

        RectTransform ghostRT = dragGhost.GetComponent<RectTransform>();
        ghostRT.localScale = originalScale;
        ghostRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, originalSizeDelta.x * 1.1f);
        ghostRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, originalSizeDelta.y * 1.1f);

        CanvasGroup ghostCG = dragGhost.GetComponent<CanvasGroup>();
        if (ghostCG == null) ghostCG = dragGhost.AddComponent<CanvasGroup>();
        ghostCG.alpha = 0.8f;
        ghostCG.blocksRaycasts = false;

        // Elevate sorting order during drag (Requested: 104)
        Canvas ghostCanvas = dragGhost.GetComponent<Canvas>();

        if (ghostCanvas != null)
        {
            defaultDragGHostSortOrder = ghostCanvas.sortingOrder;
            ghostCanvas.overrideSorting = true;
            ghostCanvas.sortingOrder = 104; 
        }
        Debug.Log("Drag Started. Default SORT="+ defaultDragGHostSortOrder);
    }

    private void UpdateItemDrag(PointerEventData eventData)
    {
        if (dragGhost == null || dragRoot == null || canvas == null) return;

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            dragRoot.GetComponent<RectTransform>(),
            eventData.position,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out localPoint
        );

        RectTransform ghostRT = dragGhost.GetComponent<RectTransform>();
        ghostRT.localPosition = localPoint;
        dragGhost.transform.SetAsLastSibling();

        // --- CLAMPING LOGIC ---
        RectTransform rootRT = dragRoot.GetComponent<RectTransform>();
        if (rootRT != null)
        {
            // 1. Try Collider Bounds
            Bounds? colliderBounds = GetCompoundColliderBounds(dragGhost.transform);
            
            if (colliderBounds.HasValue)
            {
                Bounds bounds = colliderBounds.Value;
                
                // Get Root World Bounds
                Vector3[] rootCorners = new Vector3[4];
                rootRT.GetWorldCorners(rootCorners);
                
                Vector3 rootMin = rootCorners[0];
                Vector3 rootMax = rootCorners[0];
                for (int i = 1; i < 4; i++)
                {
                    rootMin = Vector3.Min(rootMin, rootCorners[i]);
                    rootMax = Vector3.Max(rootMax, rootCorners[i]);
                }

                // --- SCREEN BOUNDS CLAMPING (ADDITION) ---
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
                    Rect safeArea = Screen.safeArea;
                    float zDepth = dragGhost.transform.position.z - cam.transform.position.z;
                    screenMin = cam.ScreenToWorldPoint(new Vector3(safeArea.xMin, safeArea.yMin, zDepth));
                    screenMax = cam.ScreenToWorldPoint(new Vector3(safeArea.xMax, safeArea.yMax, zDepth));
                }

                // Intersect Parent Bounds with Screen Bounds
                Vector3 finalMin = Vector3.Max(rootMin, screenMin);
                Vector3 finalMax = Vector3.Min(rootMax, screenMax);

                Vector3 currentPos = dragGhost.transform.position;
                Vector3 clampedPos = currentPos;

                Vector3 offset = bounds.center - currentPos;
                Vector3 extents = bounds.extents;

                Vector3 minLimit = finalMin - offset + extents;
                Vector3 maxLimit = finalMax - offset - extents;

                clampedPos.x = Mathf.Clamp(currentPos.x, minLimit.x, maxLimit.x);
                clampedPos.y = Mathf.Clamp(currentPos.y, minLimit.y, maxLimit.y);

                dragGhost.transform.position = clampedPos;
            }
            else
            {
                // 2. Fallback: RectTransform Bounds (Compound)
                Rect compoundRect = GetCompoundRect(ghostRT);
                
                Vector3 minPosition = rootRT.rect.min - compoundRect.min;
                Vector3 maxPosition = rootRT.rect.max - compoundRect.max;
                
                Vector3 pos = ghostRT.localPosition;
                pos.x = Mathf.Clamp(pos.x, minPosition.x, maxPosition.x);
                pos.y = Mathf.Clamp(pos.y, minPosition.y, maxPosition.y);
                ghostRT.localPosition = pos;
            }
        }
        
        CheckPlacement();
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

    private Rect GetCompoundRect(RectTransform root)
    {
        // Start with root's own rect
        Rect compoundRect = root.rect;
        
        // Get all children
        var children = root.GetComponentsInChildren<RectTransform>();
        Vector3[] corners = new Vector3[4];
        
        Vector2 min = compoundRect.min;
        Vector2 max = compoundRect.max;
        
        foreach (var child in children)
        {
            if (child == root) continue;
            
            // Get child corners in World Space
            child.GetWorldCorners(corners);
            
            // Convert to Root's Local Space and expand bounds
            for (int i = 0; i < 4; i++)
            {
                Vector3 localPos = root.InverseTransformPoint(corners[i]);
                if (localPos.x < min.x) min.x = localPos.x;
                if (localPos.y < min.y) min.y = localPos.y;
                if (localPos.x > max.x) max.x = localPos.x;
                if (localPos.y > max.y) max.y = localPos.y;
            }
        }
        
        return new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
    }

    private bool _isValidPlacement = true; // Default to true if no placement component
    [SerializeField] private Color validColor = Color.white;
    [SerializeField] private Color invalidColor = Color.red;



    private void CheckPlacement()
    {
        if (dragGhost == null) return;

        // USER REQUEST: Only objects with IItemBehaviours should exhibit this collision logic.
        if (dragGhost.GetComponent<IItemBehaviours>() == null)
        {
             _isValidPlacement = true;
             return;
        }

        ItemPlacement itemPlacement = dragGhost.GetComponent<ItemPlacement>();
        if (itemPlacement == null)
        {
             _isValidPlacement = true; // No restriction
             return; 
        }

        Collider2D ghostCollider = dragGhost.GetComponent<Collider2D>();
        if (ghostCollider == null)
        {
             // If no collider, we can't check placement areas via physics.
             // Assume valid? or Invalid? 
             // User said: "Placement ... determined by Item's Collider".
             // If missing, maybe warn?
             _isValidPlacement = true; 
             return;
        }

        _isValidPlacement = false;
        
        // Use Collider Overlap
        List<Collider2D> results = new List<Collider2D>();
        ContactFilter2D filter = new ContactFilter2D();
        filter.NoFilter();
        
        _isValidPlacement = false;
        bool areaFound = false;
        bool collisionDetected = false;

        int count = ghostCollider.OverlapCollider(filter, results);

        if (count > 0)
        {
            foreach (Collider2D hitCollider in results)
            {
                GameObject hitObj = hitCollider.gameObject;
                if (hitObj == dragGhost) continue;

                // 1. Check for ItemPlacement Collision (Overlap with another item)
                ItemPlacement otherPlacement = hitObj.GetComponent<ItemPlacement>();
                if (otherPlacement != null)
                {
                    // USER REQUEST: If EITHER item has PlacementType.All, they are compatible.
                    // All acts as a "Wildcard" that allows overlap.
                    if (itemPlacement.allowedType == PlacementType.All || otherPlacement.allowedType == PlacementType.All)
                    {
                         // Compatible overlap, ignore collision
                    }
                    else
                    {
                        collisionDetected = true;
                        // Debug.Log($"[ItemDragPanel] Collision with {hitObj.name}. CollisionDetected=True");
                        // We don't break here because we might need to find a valid area first to know "it would be valid if not for collision"
                        // But technically, if collisionDetected is true, result is false anyway.
                    }
                }

                // 2. Check for PlacementArea (Valid Zone)
                PlacementArea area = hitObj.GetComponent<PlacementArea>();
                if (area == null)
                {
                    area = hitObj.GetComponentInParent<PlacementArea>();
                }

                if (area != null)
                {
                    if (itemPlacement.allowedType == PlacementType.Both || 
                        itemPlacement.allowedType == area.type)
                    {
                        areaFound = true;
                    }
                }
            }
        }
        
        // Final Status: Must find a Valid Area AND Not collide with another Item
        _isValidPlacement = areaFound && !collisionDetected;


        // Apply Visuals
        UIStickerEffect[] effects = dragGhost.GetComponentsInChildren<UIStickerEffect>(true);
        if (effects != null)
        {
            foreach (var effect in effects)
            {
               if(effect != null)
               {
                   effect.SetOutlineColor(_isValidPlacement ? validColor : invalidColor);
                   effect.enabled = true;
               }
            }
        }
    }

    private void EndItemDrag()
    {
        if (dragGhost == null) return;

        // Check Placement Validity - Moved check downwards to "Clean Logic"
        
        // 1. Check for Interactions (IInteractable)
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);
        
        // ... (rest of the method)


        Debug.Log($"[ItemDragPanel] Raycast Hit Count: {results.Count}");

        foreach (var result in results)
        {
            Debug.Log($"[ItemDragPanel] Hit: {result.gameObject.name} (Layer: {result.gameObject.layer})");

            // Skip the drag ghost itself
            if (result.gameObject == dragGhost) continue;

            var interactable = result.gameObject.GetComponent<IInteractable>();
            if (interactable != null)
            {
                Debug.Log($"[ItemDragPanel] Found IInteractable on {result.gameObject.name}");
                RoomObject sourceRoomObject = dragGhost.GetComponent<RoomObject>();
                // Ensure the ghost has a RoomObject component if needed for the interface signature
                if (sourceRoomObject == null) sourceRoomObject = dragGhost.AddComponent<RoomObject>();

                if (interactable.CanInteract(sourceRoomObject))
                {
                    Debug.Log($"[ItemDragPanel] Interaction Success with {result.gameObject.name}");
                    bool consumed = interactable.OnInteract(sourceRoomObject);
                    
                    // If the interaction consumed the item, stop here (don't place it)
                    if (consumed)
                    {
                        Debug.Log("[ItemDragPanel] Item consumed by interaction. Destroying ghost.");
                        // Ensure it's destroyed if the script didn't do it (though it should have)
                        if (dragGhost != null) Destroy(dragGhost);
                        dragGhost = null;
                        return; 
                    }
                    
                    // Break after first interaction to avoid multiple triggers?
                    // Or continue? Usually one interaction per drop is safer.
                    break; 
                }
                else
                {
                     Debug.Log($"[ItemDragPanel] CanInteract returned FALSE for {result.gameObject.name}");
                }
            }
        }

        // 2. Place in IRoomPanel (Updated Logic)
        // Find which IRoomPanel we dropped onto
        IRoomPanel targetPanel = null;
        IRoomPanel[] roomPanels = FindObjectsOfType<IRoomPanel>()
            .Where(p => p.gameObject.activeInHierarchy)
            .ToArray();

        foreach (var panel in roomPanels)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(
                panel.GetComponent<RectTransform>(), 
                Input.mousePosition, 
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera))
            {
                targetPanel = panel;
                break;
            }
        }

        // Logic Check: Target Panel Found + Valid Placement (Area+NoCollision)
        if (targetPanel != null && _isValidPlacement)
        {
            CanvasGroup ghostCG = dragGhost.GetComponent<CanvasGroup>();
            ghostCG.alpha = 1f;
            ghostCG.blocksRaycasts = true;

            RectTransform ghostRT = dragGhost.GetComponent<RectTransform>();
            // Do NOT force localScale/localRotation here. Let SetParent(true) preserve world transform.
            // ghostRT.localScale = originalScale; 
            // ghostRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, originalSizeDelta.x);
            // ghostRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, originalSizeDelta.y);

            Canvas ghostCanvas = dragGhost.GetComponent<Canvas>();
            if (ghostCanvas != null)
            {
                ghostCanvas.overrideSorting = true;
                ghostCanvas.sortingOrder = defaultDragGHostSortOrder; 
            }

            // Parent to the target room's object container (or transform if container is null)
            Transform targetContainer = targetPanel.objectContainer != null ? targetPanel.objectContainer : targetPanel.transform;
            
            // Use Unity's built-in worldPositionStays=true to preserve visual position/scale
            dragGhost.transform.SetParent(targetContainer, true); 
            
            // FIX: Only flatten Z position to ensure it's not behind the background
            Vector3 localPos = dragGhost.transform.localPosition;
            localPos.z = 0;
            dragGhost.transform.localPosition = localPos;

            // FIX: Ensure Layer matches
            dragGhost.layer = targetContainer.gameObject.layer;
            
            // Register logic
            if (dragGhost.GetComponent<RoomObject>() == null)
                dragGhost.AddComponent<RoomObject>();

            // Reset sorting order
            ItemSelection itemSelection = dragGhost.GetComponent<ItemSelection>();
            if (itemSelection != null)
            {
                itemSelection.ResetSortingOrder();
            }

            // REFRESH DRAG HANDLER BASELINE
            // This is critical to prevent "jumps" on the next drag.
            // We tell DragHandler: "Forget your ghost life. You live here now. This is your home."
            DragHandler handler = dragGhost.GetComponent<DragHandler>();
            if (handler != null)
            {
                handler.UpdateCurrentPositionAsValid();
            }
            
            Debug.Log($"Item {targetPanel.name} paneline bırakıldı.");
        }
        else
        {
            // Dropped outside any room -> Cancel
            Destroy(dragGhost);
            Debug.Log("Item herhangi bir odaya bırakılmadı, iptal edildi.");
        }

        dragGhost = null;
    }
}