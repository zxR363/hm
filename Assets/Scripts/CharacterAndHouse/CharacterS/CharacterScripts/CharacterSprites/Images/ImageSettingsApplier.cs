using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

// [ExecuteAlways] // Disabled to prevent Graphic Rebuild Loop in Editor
// [RequireComponent(typeof(Image))] // DISABLED: Causes implicit AddComponent during rebuild.
public class ImageSettingsApplier : MonoBehaviour
{
    public SpriteSettingsDatabase database; // Merkezi DB
    private Image img;

    private Sprite lastSprite = null;

    void OnEnable()
    {
        Debug.Log($"[DEBUG_TRACE] {Time.frameCount} - ImageSettingsApplier {gameObject.name} - OnEnable");
        img = GetComponent<Image>();
        if (img == null) 
        {
            // Debug.LogWarning($"[ImageSettingsApplier] {name} missing Image component. Please add one.");
            return;
        }

        if (img == null) img = GetComponent<Image>();
        if (img == null) return; // Safety check
#if UNITY_EDITOR
        // Defer to avoid "Graphic Rebuild Loop" if enabled during layout
        UnityEditor.EditorApplication.delayCall += () => {
            if (this != null) 
            {
                 ApplySettings();
                 if (img != null) lastSprite = img.sprite;
            }
        };
#else
        ApplySettings();
        lastSprite = img.sprite;
#endif
    }

    void OnValidate()
    {
        // Debug.Log($"[DEBUG_TRACE] {Time.frameCount} - ImageSettingsApplier {gameObject.name} - OnValidate (Disabled for Safety)");
        // Disabled entirely to prevent Editor-time rebuild loops
    }

    void Update()
    {
#if !UNITY_EDITOR
        if (img == null) img = GetComponent<Image>();
        ApplySettingsIfSpriteChanged();
#endif
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
        Debug.Log($"[DEBUG_TRACE] {Time.frameCount} - ImageSettingsApplier {gameObject.name} - ApplySettings START");
        if (img == null || img.sprite == null || database == null) 
        {
             Debug.Log($"[DEBUG_TRACE] {Time.frameCount} - ImageSettingsApplier {gameObject.name} - ApplySettings EXIT (Missing Refs)");
             return;
        }

        var setting = database.GetSetting(img.sprite);
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

                // 🔥 Size Delta (Width/Height) Support
                if(setting.size != Vector2.zero) // Eski verileri bozma
                {
                    var rt = GetComponent<RectTransform>();
                    if(rt != null)
                    {
                         if (Vector2.SqrMagnitude(rt.sizeDelta - setting.size) > 0.0001f)
                            rt.sizeDelta = setting.size;
                    }
                }
            }
        }
    }

    public void SaveCurrentTransformForCurrentSprite()
    {
        if (img == null || img.sprite == null || database == null) return;

        var rt = GetComponent<RectTransform>();
        Vector2 size = (rt != null) ? rt.sizeDelta : Vector2.zero;

        database.SaveSetting(img.sprite, transform.localPosition, transform.localScale, size);

        #if UNITY_EDITOR
        EditorUtility.SetDirty(database);
        AssetDatabase.SaveAssets(); // 🔥 Garantiye al: Hemen diske yaz.
        #endif
    }
}