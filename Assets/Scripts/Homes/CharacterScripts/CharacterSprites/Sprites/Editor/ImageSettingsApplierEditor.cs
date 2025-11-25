using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ImageSettingsApplier))]
public class ImageSettingsApplierEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ImageSettingsApplier applier = (ImageSettingsApplier)target;

        GUILayout.Space(10);

        if (GUILayout.Button("📌 Save Current Transform for Sprite"))
        {
            applier.SaveCurrentTransformForCurrentSprite();
        }

        if (GUILayout.Button("🔄 Apply Settings"))
        {
            applier.ApplySettings();
        }
    }
}