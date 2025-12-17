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

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void ProcessDrag(Vector2 pointerScreenPos)
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

    private void LateUpdate()
    {
        // 1. Input Check
        if (!Input.GetMouseButton(0) && Input.touchCount == 0)
        {
            if (_isActive)
            {
                if(debugMode) Debug.Log("[DragAutoScroller] Stopping.");
                _isActive = false;
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
        // User requested Position-based approach to remove "Acceleration Freeze"
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

        // 6. Direct Translation (Restored)
        // Velocity approach failed (no movement), so we return to Direct Translation.
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

        // Left Edge
        if (pointerPos.x < edgeThreshold)
        {
            float rawFactor = Mathf.Clamp01((edgeThreshold - pointerPos.x) / edgeThreshold);
            float factor = Mathf.Lerp(minFactor, 1f, rawFactor);
            return maxScrollSpeed * factor; 
        }
        // Right Edge
        else if (pointerPos.x > screenWidth - edgeThreshold)
        {
            float distFromEdge = pointerPos.x - (screenWidth - edgeThreshold);
            float rawFactor = Mathf.Clamp01(distFromEdge / edgeThreshold);
            float factor = Mathf.Lerp(minFactor, 1f, rawFactor);
            return -maxScrollSpeed * factor;
        }
        
        return 0f;
    }
}