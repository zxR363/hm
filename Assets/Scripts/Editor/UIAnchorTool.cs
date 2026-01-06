using UnityEngine;
using UnityEditor;

public class UIAnchorTool : EditorWindow
{
    [MenuItem("Tools/Nibi World: My Avatar Life/Fix Selected Anchors")]
    static void FixSelectedAnchors()
    {
        int count = 0;
        foreach (GameObject go in Selection.gameObjects)
        {
            if (FixAnchor(go.GetComponent<RectTransform>()))
            {
                count++;
            }
            
            // Also do children? User asked for "All objects".
            // Let's do children recursively if selected
            RectTransform[] children = go.GetComponentsInChildren<RectTransform>(true);
            foreach (var child in children)
            {
                if (child.gameObject != go && FixAnchor(child))
                {
                    count++;
                }
            }
        }
        Debug.Log($"[UIAnchorTool] Fixed anchors for {count} objects!");
    }

    [MenuItem("Tools/Nibi World: My Avatar Life/Fix ALL Anchors in Scene (Use with Caution)")]
    static void FixAllAnchorsInScene()
    {
        // Find all RectTransforms in scene
        RectTransform[] allRects = Resources.FindObjectsOfTypeAll<RectTransform>();
        int count = 0;
        foreach (var rt in allRects)
        {
            // Only process scene objects (hide flags check)
            if (rt.gameObject.scene.name == null) continue; // Skip assets
            
            if (FixAnchor(rt))
            {
                count++;
            }
        }
        Debug.Log($"[UIAnchorTool] Fixed anchors for {count} objects in the scene!");
    }

    static bool FixAnchor(RectTransform rt)
    {
        if (rt == null) return false;
        
        RectTransform parentRt = rt.parent as RectTransform;
        if (parentRt == null) return false;

        // Don't fix key UI elements that SHOULD be static or are part of specific layout groups?
        // LayoutGroups control their children, setting anchors might conflict or be overwritten.
        // But for "StablePosition" use case (Manual placement), this is fine.
        if (rt.GetComponentInParent<UnityEngine.UI.LayoutGroup>() != null && rt.parent.GetComponent<UnityEngine.UI.LayoutGroup>() != null)
        {
            // If direct child of LayoutGroup, skip
            return false;
        }

        Undo.RecordObject(rt, "Fix UI Anchor");

        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);
        // corners[0] = bottom-left, corners[2] = top-right

        // Calculate anchors relative to parent
        Vector2 minLocal = parentRt.InverseTransformPoint(corners[0]);
        Vector2 maxLocal = parentRt.InverseTransformPoint(corners[2]);

        Vector2 minAnchor = Rect.PointToNormalized(parentRt.rect, minLocal);
        Vector2 maxAnchor = Rect.PointToNormalized(parentRt.rect, maxLocal);

        // Clamp to 0-1 to avoid weird off-screen anchors if desired, but usually we want exact.
        
        // Check if values are valid (not infinity/NaNf)
        if (float.IsNaN(minAnchor.x) || float.IsInfinity(minAnchor.x)) return false;

        rt.anchorMin = minAnchor;
        rt.anchorMax = maxAnchor;
        
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        
        // Remove StablePosition script if exists
        var stable = rt.GetComponent("StablePositionOnce") as MonoBehaviour; // Reflection to avoid dependency if file renamed
        if (stable == null) stable = rt.GetComponent("StablePosition") as MonoBehaviour;
        
        if (stable != null)
        {
            Undo.DestroyObjectImmediate(stable);
        }

        return true;
    }
}
