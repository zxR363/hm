using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class ResponsiveRectHelper : MonoBehaviour
{
    [Header("Settings")]
    [Header("Settings")]
    [Tooltip("If true, updates size every frame in Editor (useful for testing). In build, updates on event.")]
    public bool liveUpdate = true;
    public bool ignoreLayoutGroups = false; // Safety Bypass

    [Header("Responsive Size")]
    [Tooltip("Width as a percentage of Parent Width (0.5 = 50%)")]
    [Range(0f, 1f)]
    public float widthPercent = 1f;

    [Tooltip("Height as a percentage of Parent Height (0.5 = 50%)")]
    [Range(0f, 1f)]
    public float heightPercent = 1f;

    [Header("Constraints")]
    public bool lockAspectRatio = false;
    [Tooltip("Width / Height. If 0, uses current aspect ratio on Init.")]
    public float targetAspectRatio = 1.77f; // 16:9 Default
    [Tooltip("If locked, which axis controls the size?")]
    public AspectMode aspectMode = AspectMode.WidthControlsHeight;

    public enum AspectMode
    {
        WidthControlsHeight,
        HeightControlsWidth,
        FitInParent,
        EnvelopeParent
    }

    private RectTransform _rect;
    private RectTransform _parentRect;

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
    }

    private void Start()
    {
        UpdateSizing();
    }

    private void OnEnable()
    {
        UpdateSizing();
    }

    // Called when dimensions change (e.g. screen resize)
    private void OnRectTransformDimensionsChange()
    {
        if (enabled && gameObject.activeInHierarchy)
        {
            UpdateSizing();
        }
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (liveUpdate && !Application.isPlaying)
        {
            UpdateSizing();
        }
    }
#endif

    public void UpdateSizing()
    {
        if (_rect == null) _rect = GetComponent<RectTransform>();
        if (_rect.parent == null) return;
        
        _parentRect = _rect.parent as RectTransform;
        _parentRect = _rect.parent as RectTransform;
        if (_parentRect == null) return;

        // FIX: Detect Layout Group Conflict
        // If the parent has a LayoutGroup (Horizontal/Vertical/Grid) that controls this child,
        // we should NOT override the size. LayoutGroup wins.
        // UNLESS disableLayoutCheck is enabled (power user override).
        if (!ignoreLayoutGroups)
        {
             // Check if parent has a layout group that controls width/height
             var lg = _parentRect.GetComponent<UnityEngine.UI.LayoutGroup>();
             if (lg != null && lg.enabled)
             {
                 // Ideally check exact axis control, but generic disable is safer for now.
                 // Debug.LogWarning($"[ResponsiveRectHelper] Disabled on {name} because Parent {parent.name} has a LayoutGroup.");
                 return;
             }
        }

        float parentW = _parentRect.rect.width;
        float parentH = _parentRect.rect.height;

        if (parentW <= 0 || parentH <= 0) return;

        // Calculate Target Sizes
        float targetW = parentW * widthPercent;
        float targetH = parentH * heightPercent;

        if (lockAspectRatio)
        {
            float ratio = targetAspectRatio > 0 ? targetAspectRatio : 1f;
            
            // If aspect ratio is 0 or uninitialized, try to grab from current
            if (targetAspectRatio <= 0.01f && _rect.rect.height > 0)
            {
                 targetAspectRatio = _rect.rect.width / _rect.rect.height;
                 ratio = targetAspectRatio;
            }

            switch (aspectMode)
            {
                case AspectMode.WidthControlsHeight:
                    // Width is authoritative (based on percent), Height is calculated
                    targetH = targetW / ratio;
                    break;

                case AspectMode.HeightControlsWidth:
                    // Height is authoritative, Width is calculated
                    targetW = targetH * ratio;
                    break;
                
                case AspectMode.FitInParent:
                    // Fit entirely inside parent while keeping aspect ratio
                    // Try Width First
                    float testH = targetW / ratio;
                    if (testH <= parentH * heightPercent)
                    {
                        targetH = testH;
                    }
                    else
                    {
                        // Width was too wide, constrain by Height
                        targetH = parentH * heightPercent;
                        targetW = targetH * ratio;
                    }
                    break;

                 case AspectMode.EnvelopeParent:
                    // Cover parent completely (Zoom/Crop effect)
                    // Try Width First
                    float testH2 = targetW / ratio;
                    if (testH2 >= parentH * heightPercent)
                    {
                        targetH = testH2;
                    }
                    else
                    {
                        targetH = parentH * heightPercent;
                        targetW = targetH * ratio;
                    }
                    break;
            }
        }

        // Apply Size (Using SetSizeWithCurrentAnchors to avoid Anchor confusion)
        if (Mathf.Abs(_rect.rect.width - targetW) > 0.1f)
            _rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetW);
        
        if (Mathf.Abs(_rect.rect.height - targetH) > 0.1f)
            _rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetH);
        
        // Optional: Force Center Anchor? 
        // For now, we rely on user setting Anchors to Center (0.5, 0.5) if they want centered scaling.
    }
}
