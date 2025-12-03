using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
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
        ApplySettingsIfSpriteChanged();
    }

    void Update()
    {
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
            transform.localPosition = setting.position;
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