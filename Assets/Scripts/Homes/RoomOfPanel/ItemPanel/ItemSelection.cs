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
    
    private int defaultSortingOrder;
    private bool isSortingOverridden = false;

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
            defaultSortingOrder = canvas.sortingOrder;

            // Ensure GraphicRaycaster exists if Canvas is present
            if (GetComponent<UnityEngine.UI.GraphicRaycaster>() == null)
            {
                gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }
            
            // Override sorting to be on top of the panel
            if (ItemSelectionPanelController.Instance != null)
            {
                canvas.overrideSorting = true;
                canvas.sortingOrder = ItemSelectionPanelController.Instance.ContentSortingOrder + 1;
                isSortingOverridden = true;
            }
        }

        EnsureRaycastTarget();
    }

    private void EnsureRaycastTarget()
    {
        // Items need a graphic to be clickable/draggable.
        // If the item visual is a child, the root might not have an image.
        // We ensure there's a transparent image on the root to catch events.
        Image img = GetComponent<Image>();
        if (img == null)
        {
            img = gameObject.AddComponent<Image>();
            img.color = Color.clear; // Fully transparent
        }
        img.raycastTarget = true;
    }

    public void ResetSortingOrder()
    {
        if (canvas != null && isSortingOverridden)
        {
            canvas.sortingOrder = defaultSortingOrder;
        }
    }

    private void Start()
    {
        // Find parent ScrollRect to get the correct Viewport
        ScrollRect scrollRect = GetComponentInParent<ScrollRect>();
        if (scrollRect != null)
        {
            viewPortRect = scrollRect.viewport;
            if (viewPortRect == null)
            {
                 Transform viewportTransform = transform.parent?.parent;
                 if (viewportTransform != null) viewPortRect = viewportTransform.GetComponent<RectTransform>();
            }
        }
        else
        {
             Transform viewportTransform = transform.parent?.parent;
             if (viewportTransform != null) viewPortRect = viewportTransform.GetComponent<RectTransform>();
        }
        
        // Optimization: Disable manual visibility check to avoid CPU overhead during scroll.
        // Render everything at once.
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
        }
    }

    // Removed CheckVisibilityRoutine and IsVisibleInViewport for performance



}


