using UnityEngine;
using UnityEngine.UI;
using System.Text;

public class SceneHierarchyDumper : MonoBehaviour
{
    public GameObject rootTarget;

    [ContextMenu("Dump Hierarchy Info")]
    public void DumpInfo()
    {
        if (rootTarget == null) rootTarget = this.gameObject;
        
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("=== HIERARCHY DUMP ===");
        
        DumpRecursive(rootTarget.transform, 0, sb);
        
        Debug.Log(sb.ToString());
    }

    private void DumpRecursive(Transform t, int depth, StringBuilder sb)
    {
        string indent = new string('-', depth * 2);
        
        string info = $"{indent}{t.name}";
        
        if (t.TryGetComponent<RectTransform>(out var rect))
        {
            info += $" [Rect: Pos={rect.anchoredPosition}, Size={rect.sizeDelta}, " +
                    $"Anchors=({rect.anchorMin.x:F2},{rect.anchorMin.y:F2})-({rect.anchorMax.x:F2},{rect.anchorMax.y:F2}), " +
                    $"Pivot={rect.pivot}, WorldPos={t.position}]";
        }
        
        if (t.TryGetComponent<CanvasScaler>(out var scaler))
        {
            info += $" [Scaler: Mode={scaler.uiScaleMode}, RefRes={scaler.referenceResolution}, Match={scaler.matchWidthOrHeight}]";
        }
        
        if (t.TryGetComponent<LayoutGroup>(out var lg))
        {
            info += $" [LayoutGroup: {lg.GetType().Name}]";
        }

        if (t.TryGetComponent<ContentSizeFitter>(out var csf))
        {
            info += $" [ContentSizeFitter: H={csf.horizontalFit} V={csf.verticalFit}]";
        }

        sb.AppendLine(info);

        foreach (Transform child in t)
        {
            DumpRecursive(child, depth + 1, sb);
        }
    }

    private void Start()
    {
        // Auto-dump on start if attached
        DumpInfo();
    }
}
