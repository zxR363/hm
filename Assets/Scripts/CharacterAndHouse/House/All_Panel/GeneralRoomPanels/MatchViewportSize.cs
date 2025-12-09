using UnityEngine;
using UnityEngine.UI;

// Odaların scale edilmesini sağlıyor.

[RequireComponent(typeof(LayoutElement))]
[ExecuteInEditMode] // Runs in editor to see changes immediately
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

    private void OnRectTransformDimensionsChange()
    {
        UpdateSize();
    }

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
            layoutElement.preferredWidth = parentViewport.rect.width;
            layoutElement.preferredHeight = parentViewport.rect.height;
        }
    }
}
