using UnityEngine;
using UnityEngine.UI;

public class ItemSelection : MonoBehaviour
{
    // Removed Canvas dependency as it breaks ScrollRect
    // [SerializeField] private int targetSortingOrder = 0; 
    
    private RectTransform itemRect;
    private CanvasGroup canvasGroup;

    private RectTransform viewPortRect;
    private Canvas canvas;

    [SerializeField] private Vector3 defaultScale = Vector3.one;

    private void Awake()
    {
        itemRect = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponent<Canvas>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        
        transform.localScale = defaultScale;

        if (canvas != null)
        {
            // Ensure GraphicRaycaster exists if Canvas is present, otherwise interaction might be weird
            if (GetComponent<UnityEngine.UI.GraphicRaycaster>() == null)
            {
                gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }
        }
    }

    private void Start()
    {
        // Find Viewport (assuming Viewport -> Content -> Item)
        Transform viewportTransform = transform.parent?.parent;
        if (viewportTransform != null)
        {
            viewPortRect = viewportTransform.GetComponent<RectTransform>();
        }
    }

    private void Update()
    {
        if (viewPortRect == null) return;

        // Manual culling/masking because nested Canvases break RectMask2D
        bool isVisible = IsVisibleInViewport();
        
        // Toggle visibility
        canvasGroup.alpha = isVisible ? 1f : 0f;
        canvasGroup.blocksRaycasts = isVisible;
    }

    private bool IsVisibleInViewport()
    {
        // Simple check: overlaps with viewport?
        // We can use world corners
        Vector3[] itemCorners = new Vector3[4];
        itemRect.GetWorldCorners(itemCorners);

        Vector3[] viewportCorners = new Vector3[4];
        viewPortRect.GetWorldCorners(viewportCorners);

        // Check if any corner of item is inside viewport rect (in screen space)
        // Or simpler: Check if bounds intersect
        // Let's use RectTransformUtility for accuracy with cameras
        
        Camera cam = null;
        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            cam = canvas.worldCamera;
        
        foreach (var corner in itemCorners)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(viewPortRect, corner, cam))
                return true;
        }
        
        // Also check if viewport corners are inside item (item covers viewport)
        // But usually items are smaller. 
        // Let's stick to the corner check for now, it's what was there.
        return false;
    }
}
