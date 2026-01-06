using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways] // Disabled to prevent Graphic Rebuild Loop in Editor
[RequireComponent(typeof(Image))]
public class ImageSettingsApplier : MonoBehaviour
{
    public SpriteSettingsDatabase database; // Merkezi DB
    private Image img;

    private Sprite lastSprite = null;

    void OnEnable()
    {
        if (img == null) img = GetComponent<Image>();
        ApplySettings();
        lastSprite = img.sprite;
    }

    void OnValidate()
    {
        if (img == null) img = GetComponent<Image>();
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
        
        if (img == null) img = GetComponent<Image>();
        ApplySettingsIfSpriteChanged();
        
    }

    void ApplySettingsIfSpriteChanged()
    {
        if (img == null || img.sprite == null || database == null) return;

        if (img.sprite != lastSprite)
        {
            ApplySettings();
            lastSprite = img.sprite;
        }
    }

    public void ApplySettings()
    {
        if (img == null || img.sprite == null || database == null) return;

        var setting = database.GetSetting(img.sprite);
        if (setting != null)
        {
            // Only apply if changed to prevent dirty loop in Editor
            if (Vector3.SqrMagnitude(transform.localPosition - setting.position) > 0.0001f)
                transform.localPosition = setting.position;

            if (Vector3.SqrMagnitude(transform.localScale - setting.scale) > 0.0001f)
                transform.localScale = setting.scale;
        }
    }

    public void SaveCurrentTransformForCurrentSprite()
    {
        if (img == null || img.sprite == null || database == null) return;

        database.SaveSetting(img.sprite, transform.localPosition, transform.localScale);

        #if UNITY_EDITOR
        EditorUtility.SetDirty(database);
        #endif
    }
}