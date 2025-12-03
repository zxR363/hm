using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
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
        ApplySettingsIfSpriteChanged();
    }

    void Update()
    {
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
            transform.localPosition = setting.position;
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
