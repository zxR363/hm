using UnityEngine;
using UnityEngine.UI;

// Odaların scale edilmesini sağlıyor.

[RequireComponent(typeof(LayoutElement))]
// [ExecuteInEditMode] // Runs in editor to see changes immediately -- DISABLED: Causes layout loops
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
        // UpdateSize(); // Disabled: Conflicts with Responsive Anchors
    }

    private void OnRectTransformDimensionsChange()
    {
        // UpdateSize(); // Disabled: Conflicts with Responsive Anchors
    }

/*
#if UNITY_EDITOR
    private void Update()
    {
        // Update in editor mode for preview
        if (!Application.isPlaying)
        {
            UpdateSize();
        }
    }
#endif
*/

    public void UpdateSize()
    {
        if (layoutElement == null) layoutElement = GetComponent<LayoutElement>();
        
        // Find the viewport (usually the grandparent or great-grandparent in a ScrollView)
        // Hierarchy: ScrollView -> Viewport -> Content -> RoomPanel (This Object)
        // So Viewport is parent.parent
        if (parentViewport == null && transform.parent != null && transform.parent.parent != null)
        {
            parentViewport = transform.parent.parent.GetComponent<RectTransform>();
        }

        if (parentViewport != null)
        {
            // Set preferred size to match the viewport's rect width/height
            if (Mathf.Abs(layoutElement.preferredWidth - parentViewport.rect.width) > 0.1f)
                layoutElement.preferredWidth = parentViewport.rect.width;
                
            if (Mathf.Abs(layoutElement.preferredHeight - parentViewport.rect.height) > 0.1f)
                layoutElement.preferredHeight = parentViewport.rect.height;
        }
    }
}
