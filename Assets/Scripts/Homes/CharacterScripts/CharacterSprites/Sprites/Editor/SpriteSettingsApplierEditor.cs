using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SpriteSettingsApplier))]
public class SpriteSettingsApplierEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SpriteSettingsApplier applier = (SpriteSettingsApplier)target;

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
