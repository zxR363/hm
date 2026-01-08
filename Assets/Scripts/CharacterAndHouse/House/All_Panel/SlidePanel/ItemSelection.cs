using UnityEngine;
using UnityEngine.UI;

//SlidePanel'den seçilen Item'lar için kullandğımız script

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
        {
             // READ-ONLY
        }
        
        transform.localScale = defaultScale;

        if (canvas != null)
        {
            defaultSortingOrder = canvas.sortingOrder;

            // Ensure GraphicRaycaster exists if Canvas is present
            // READ-ONLY
            {
                if (gameObject.GetComponent<UnityEngine.UI.GraphicRaycaster>() == null)
                {
                    // Debug.LogWarning("Missing GraphicRaycaster!");
                }
            }
            
            // Override sorting to be on top of the panel
            if (ItemSelectionPanelController.Instance != null)
            {
                // This might be risky if ItemSelectionPanelController isn't ready, but generally Instance is set in Awake.
                if (canvas != null)
                {
                    canvas.overrideSorting = true;
                    // canvas.sortingOrder = ItemSelectionPanelController.Instance.ContentSortingOrder + 1; // Accessing Instance in Awake can be racey
                    // Defer this too? No, property access is usually fine if instance exists.
                    // But to be safe against loops:
                }
                isSortingOverridden = true;
            }
        }

        EnsureRaycastTarget();
    }

    private void EnsureRaycastTarget()
    {
        // Items need a graphic to be clickable/draggable.
        // READ-ONLY: Do not add components dynamically to avoid Rebuild Loops.
        Image img = GetComponent<Image>();
        if (img == null)
        {
             // Debug.LogWarning($"[ItemSelection] Object {name} missing Image component! Please add it to the Prefab.");
        }
        else
        {
             img.raycastTarget = true;
        }
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
}
