using UnityEngine;
using UnityEngine.UI;

public class DebugAnchorLogger : MonoBehaviour
{
    public RectTransform[] targets;
    public bool showOnScreen = true;

    private void OnGUI()
    {
        if (!showOnScreen) return;

        GUIStyle style = new GUIStyle();
        style.fontSize = 24; // Large for mobile
        style.normal.textColor = Color.red; // Visible on white/dark

        float y = 50;
        
        GUI.Label(new Rect(20, y, Screen.width, 50), $"Screen: {Screen.width}x{Screen.height} (SafeArea:{Screen.safeArea})", style);
        y += 40;

        if (targets != null)
        {
            foreach (var t in targets)
            {
                if (t == null) continue;
                string info = $"{t.name}:\n" +
                              $"  Rect: {t.rect}\n" +
                              $"  Anch: {t.anchorMin}-{t.anchorMax}\n" +
                              $"  Off: {t.offsetMin}-{t.offsetMax}\n" +
                              $"  Scale: {t.localScale}\n" +
                              $"  Pos: {t.localPosition}";
                
                GUI.Label(new Rect(20, y, Screen.width, 200), info, style);
                y += 180;
            }
        }
    }
}
