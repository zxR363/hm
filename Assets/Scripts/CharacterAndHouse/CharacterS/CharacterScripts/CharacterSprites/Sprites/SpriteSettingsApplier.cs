using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways] // Disabled to prevent Graphic Rebuild Loop in Editor
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteSettingsApplier : MonoBehaviour
{
    public SpriteSettingsDatabase database; // Merkezi DB
    private SpriteRenderer sr;

    private Sprite lastSprite = null;

    void OnEnable()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        ApplySettings();
        lastSprite = sr.sprite;
    }

    void OnValidate()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.delayCall += () => {
            if (this != null) ApplySettingsIfSpriteChanged();
        };
#else
        ApplySettingsIfSpriteChanged();
#endif
    }

    void Update()
    {
        // DISABLED for Layout Debugging
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        ApplySettingsIfSpriteChanged();
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
            if (Vector3.SqrMagnitude(transform.localPosition - setting.position) > 0.0001f)
                transform.localPosition = setting.position;

            if (Vector3.SqrMagnitude(transform.localScale - setting.scale) > 0.0001f)
                transform.localScale = setting.scale;
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
