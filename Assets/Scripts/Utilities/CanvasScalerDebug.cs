using UnityEngine;
using UnityEngine.UI;

public class CanvasScalerDebug : MonoBehaviour
{
    public CanvasScaler targetScaler;
    private Rect windowRect = new Rect(20, 350, 450, 200);

    private void Start()
    {
        if (targetScaler == null) targetScaler = GetComponentInParent<CanvasScaler>();
    }

    private void OnGUI()
    {
        windowRect = GUI.Window(GetHashCode(), windowRect, DrawStats, "Canvas Scaler Debug");
    }

    private void DrawStats(int windowID)
    {
        GUILayout.BeginVertical();

        GUILayout.Label($"Screen Resolution: {Screen.width} x {Screen.height}");
        GUILayout.Label($"Safe Area: {Screen.safeArea}");
        
        if (targetScaler != null)
        {
            GUILayout.Label($"Ui Scale Mode: {targetScaler.uiScaleMode}");
            if (targetScaler.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize)
            {
                GUILayout.Label($"Reference Resolution: {targetScaler.referenceResolution}");
                GUILayout.Label($"Match Width Or Height: {targetScaler.matchWidthOrHeight}");
            }
            GUILayout.Label($"Scale Factor: {targetScaler.scaleFactor}");
        }
        else
        {
            GUILayout.Label("NO CANVAS SCALER FOUND!");
        }

        GUI.DragWindow();
        GUILayout.EndVertical();
    }
}
