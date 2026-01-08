using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

// [ExecuteAlways] // Disabled to prevent Graphic Rebuild Loop in Editor
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteSettingsApplier : MonoBehaviour
{
    public SpriteSettingsDatabase database; // Merkezi DB
    private SpriteRenderer sr;

    private Sprite lastSprite = null;

    void OnEnable()
    {
        Debug.Log($"[DEBUG_TRACE] {Time.frameCount} - SpriteSettingsApplier {gameObject.name} - OnEnable");
        if (sr == null) sr = GetComponent<SpriteRenderer>();
#if UNITY_EDITOR
        // Defer to avoid "Graphic Rebuild Loop" if enabled during layout
        UnityEditor.EditorApplication.delayCall += () => {
            if (this != null) 
            {
                 ApplySettings();
                 if (sr != null) lastSprite = sr.sprite;
            }
        };
#else
        ApplySettings();
        lastSprite = sr.sprite;
#endif
    }

    void OnValidate()
    {
        // Debug.Log($"[DEBUG_TRACE] {Time.frameCount} - SpriteSettingsApplier {gameObject.name} - OnValidate (Disabled for Safety)");
        // Disabled entirely to prevent Editor-time rebuild loops
    }

    void Update()
    {
#if !UNITY_EDITOR
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        ApplySettingsIfSpriteChanged();
#endif
    }

    void ApplySettingsIfSpriteChanged()
    {
        if (sr == null || sr.sprite == null || database == null) return;

        if (sr.sprite != lastSprite)
        {
            ApplySettings();
            lastSprite = sr.sprite;
        }
    }

    public void ApplySettings()
    {
        if (sr == null || sr.sprite == null || database == null) return;

        var setting = database.GetSetting(sr.sprite);
        if (setting != null)
        {
            // Only apply if changed to prevent dirty loop in Editor

            // FIX: If parent has a LayoutGroup, it controls the position. We should NOT overwrite it.
            // also we shouldn't overwrite Scale because things like ItemSelectionPanelController might animate it.
            bool drivenByLayout = transform.parent != null && transform.parent.GetComponent<LayoutGroup>() != null;

            if (!drivenByLayout)
            {
                if (Vector3.SqrMagnitude(transform.localPosition - setting.position) > 0.0001f)
                    transform.localPosition = setting.position;

                if (Vector3.SqrMagnitude(transform.localScale - setting.scale) > 0.0001f)
                    transform.localScale = setting.scale;
            }
        }
    }

    // Editörde kaydetmek için
    public void SaveCurrentTransformForCurrentSprite()
    {
        if (sr == null || sr.sprite == null || database == null) return;

        database.SaveSetting(sr.sprite, transform.localPosition, transform.localScale);

        #if UNITY_EDITOR
        EditorUtility.SetDirty(database); // DB asset değişikliğini kaydet
        #endif
    }
}
