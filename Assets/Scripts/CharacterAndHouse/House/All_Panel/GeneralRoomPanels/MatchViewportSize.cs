using UnityEngine;
using UnityEngine.UI;

// Force this element (RoomPanel) to match the size of the Scroll Viewport
// This ensures that "1 Page" = "1 Screen" regardless of resolution.
[RequireComponent(typeof(LayoutElement))]
[ExecuteAlways] 
public class MatchViewportSize : MonoBehaviour
{
    private RectTransform myRect;
    private RectTransform parentViewport;
    private LayoutElement layoutElement;

    private void Awake()
    {
        myRect = GetComponent<RectTransform>();
        layoutElement = GetComponent<LayoutElement>();
    }

    private void Start()
    {
        UpdateSize();
    }

    private void OnEnable()
    {
        UpdateSize();
    }

    private void OnRectTransformDimensionsChange()
    {
        if (isActiveAndEnabled)
            UpdateSize();
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (!Application.isPlaying) UpdateSize();
    }
#endif

    public void UpdateSize()
    {
        if (layoutElement == null) layoutElement = GetComponent<LayoutElement>();
        
        // Find Viewport: Standard is ScrollView -> Viewport -> Content -> Page(This)
        // So Viewport is parent.parent
        if (parentViewport == null && transform.parent != null && transform.parent.parent != null)
        {
            parentViewport = transform.parent.parent.GetComponent<RectTransform>();
        }

        if (parentViewport != null)
        {
            float targetW = parentViewport.rect.width;
            float targetH = parentViewport.rect.height; // Height match is also good for full height rooms

            // SAFETY CHECK: Only update if difference is > 1 pixel to avoid infinite layout loops
            if (Mathf.Abs(layoutElement.preferredWidth - targetW) > 1f)
            {
                layoutElement.preferredWidth = targetW;
            }
                
            if (Mathf.Abs(layoutElement.preferredHeight - targetH) > 1f)
            {
                layoutElement.preferredHeight = targetH;
            }
        }
    }
}
