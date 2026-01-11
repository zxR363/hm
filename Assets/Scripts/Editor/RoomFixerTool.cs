#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class ResponsiveSetupTool : EditorWindow
{
    [MenuItem("Tools/Make Selection Responsive (Stretch Full)")]
    public static void MakeSelectionResponsive()
    {
        GameObject target = Selection.activeGameObject;
        if (target == null)
        {
            Debug.LogError("[ResponsiveSetupTool] Please select a UI GameObject first!");
            return;
        }

        RectTransform rect = target.GetComponent<RectTransform>();
        if (rect == null)
        {
            Debug.LogError("[ResponsiveSetupTool] Selected object must have a RectTransform!");
            return;
        }

        Undo.RegisterCompleteObjectUndo(target, "Make Responsive");
        Undo.RegisterCompleteObjectUndo(rect, "Make Responsive Rect");

        // 1. Reset Anchors to Stretch-Stretch (0,0 to 1,1)
        // This visualizes "Fill Parent" nicely in Editor even without the script running
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero; // Reset offsets to 0 ensures it perfectly fills transparently
        rect.anchoredPosition = Vector2.zero;

        // 2. Add/Get Helper
        /*
        ResponsiveRectHelper res = target.GetComponent<ResponsiveRectHelper>();
        if (res == null) res = Undo.AddComponent<ResponsiveRectHelper>(target);

        // 3. Configure Defaults
        res.widthPercent = 1f; // 100%
        res.heightPercent = 1f; // 100%
        res.lockAspectRatio = false; // User can enable if needed for images
        res.aspectMode = ResponsiveRectHelper.AspectMode.EnvelopeParent;
        res.liveUpdate = true;
        
        // 4. Force Update
        res.UpdateSizing();
        */

        Debug.Log($"[ResponsiveSetupTool] Made '{target.name}' Responsive (100% Fill). You can adjust % in the Inspector.");
    }
}
#endif
