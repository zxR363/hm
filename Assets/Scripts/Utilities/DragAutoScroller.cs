using UnityEngine;
using UnityEngine.UI;

public class DragAutoScroller : MonoBehaviour
{
    public static DragAutoScroller Instance { get; private set; }

    [Header("Target")]
    public ScrollRect targetScrollRect;

    [Header("Settings")]
    public float edgeThreshold = 100f;
    public float maxScrollSpeed = 1500f; 
    
    [Header("Debug")]
    public bool debugMode = false;

    private bool _isActive = false;
    private bool _wasInertiaEnabled;
    private RectTransform _currentItem; // Helper to track large items

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void ProcessDrag(Vector2 pointerScreenPos)
    {
        _currentItem = null; 
        ProcessDragInternal(pointerScreenPos);
    }

    public void ProcessDrag(RectTransform item)
    {
        _currentItem = item;
        // We use the item to calculate edges, but pointer is fallback/trigger
        ProcessDragInternal(Input.mousePosition);
    }

    private void ProcessDragInternal(Vector2 pointerScreenPos)
    {
        if (!_isActive)
        {
             if(debugMode) Debug.Log($"[DragAutoScroller] WAKING UP!");
             _isActive = true;
             
             if (targetScrollRect != null)
             {
                 _wasInertiaEnabled = targetScrollRect.inertia;
                 targetScrollRect.inertia = false; // Turn off Unity Physics
             }
        }
    }

    // UPDATED: Changed from LateUpdate to Update to run BEFORE DragHandler.LateUpdate logic
    // This prevents "Jitter" where object updates position based on OLD parent position, then parent moves.
    private void Update()
    {
        // 1. Input Check
        if (!Input.GetMouseButton(0) && Input.touchCount == 0)
        {
            if (_isActive)
            {
                if(debugMode) Debug.Log("[DragAutoScroller] Stopping.");
                _isActive = false;
                _currentItem = null;
                if (targetScrollRect != null) 
                {
                    targetScrollRect.velocity = Vector2.zero; // Stop
                    targetScrollRect.inertia = _wasInertiaEnabled; // Restore
                }
            }
            return;
        }

        if (!_isActive || targetScrollRect == null || targetScrollRect.content == null) return;

        // 3. Calculate Immediate Frame Step (Position Delta)
        float speed = CalculateRawSpeed(Input.mousePosition);
        
        // 4. Zero Check
        if (speed == 0f) return;

        // 5. Bounds Check & Soft Landing
        float normPos = targetScrollRect.horizontalNormalizedPosition;
        float brakeThreshold = 0.05f; 

        if (speed > 0) // Going Right
        {
            if (normPos <= 0.001f) speed = 0f;
            else if (normPos < brakeThreshold)
            {
                float brakeFactor = normPos / brakeThreshold;
                speed *= brakeFactor;
            }
        }
        else if (speed < 0) // Going Left
        {
            if (normPos >= 0.999f) speed = 0f;
            else if (normPos > (1f - brakeThreshold))
            {
                float remaining = 1f - normPos;
                float brakeFactor = remaining / brakeThreshold;
                speed *= brakeFactor;
            }
        }

        // 6. Direct Translation
        if (speed != 0f)
        {
            float dt = Time.unscaledDeltaTime;
            Vector2 pos = targetScrollRect.content.anchoredPosition;
            pos.x += speed * dt; // Move directly based on frame speed
            targetScrollRect.content.anchoredPosition = pos;
        }
    }

    private float CalculateRawSpeed(Vector2 pointerPos)
    {
        float screenWidth = Screen.width;
        float minFactor = 0.7f; // Start strong

        // Initialize with Pointer Pos
        float leftEdgeVal = pointerPos.x;
        float rightEdgeVal = pointerPos.x;

        // NEW: If Item Provided, Check Compound Visual Bounds (Children included)
        // This ensures that if a child Image is larger than the parent, it triggers the scroll.
        if (_currentItem != null)
        {
            float minX = float.MaxValue;
            float maxX = float.MinValue;

            // Get ALL RectTransforms (including root)
            var rects = _currentItem.GetComponentsInChildren<RectTransform>();
            Vector3[] corners = new Vector3[4];
            
            // Camera setup
            Canvas c = _currentItem.GetComponentInParent<Canvas>();
            Camera cam = (c != null && c.renderMode != RenderMode.ScreenSpaceOverlay) ? c.worldCamera : null;
            if (cam == null) cam = Camera.main;
            bool isScreenSpaceOverlay = (c != null && c.renderMode == RenderMode.ScreenSpaceOverlay);

            foreach (var rt in rects)
            {
                // Optimization: Ignore non-visible or zero-scale items? 
                // For now, check all.
                
                rt.GetWorldCorners(corners);
                for(int i=0; i<4; i++)
                {
                    Vector2 screenPoint = corners[i];
                    if (!isScreenSpaceOverlay && cam != null)
                    {
                        screenPoint = cam.WorldToScreenPoint(corners[i]);
                    }
                    
                    if (screenPoint.x < minX) minX = screenPoint.x;
                    if (screenPoint.x > maxX) maxX = screenPoint.x;
                }
            }
            
            // Combined Logic:
            // Left Scroll: Use Left-most child edge vs Left Screen Edge
            leftEdgeVal = Mathf.Min(leftEdgeVal, minX);
            
            // Right Scroll: Use Right-most child edge vs Right Screen Edge
            rightEdgeVal = Mathf.Max(rightEdgeVal, maxX);
        }

        // Left Edge Logic
        if (leftEdgeVal < edgeThreshold)
        {
            float rawFactor = Mathf.Clamp01((edgeThreshold - leftEdgeVal) / edgeThreshold);
            float factor = Mathf.Lerp(minFactor, 1f, rawFactor);
            return maxScrollSpeed * factor; 
        }
        // Right Edge Logic
        else if (rightEdgeVal > screenWidth - edgeThreshold)
        {
            float distFromEdge = rightEdgeVal - (screenWidth - edgeThreshold);
            float rawFactor = Mathf.Clamp01(distFromEdge / edgeThreshold);
            float factor = Mathf.Lerp(minFactor, 1f, rawFactor);
            return -maxScrollSpeed * factor;
        }
        
        return 0f;
    }
}