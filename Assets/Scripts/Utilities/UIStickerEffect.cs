using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
[ExecuteInEditMode]
public class UIStickerEffect : MonoBehaviour
{
    [Header("Settings")]
    public Color outlineColor = Color.white;
    [Range(0, 50)]
    public float outlineWidth = 2f;

    private Image _image;
    private Material _material;
    private Shader _shader;

    private void OnEnable()
    {
        _image = GetComponent<Image>();
        _shader = Shader.Find("UI/UISticker");

        if (_shader == null)
        {
            Debug.LogError("[UIStickerEffect] Shader 'UI/UISticker' not found!");
            return;
        }

        // Create a dynamic material instance
        _material = new Material(_shader);
        _image.material = _material;
        
        UpdateMaterial();
    }

    private void OnDisable()
    {
        if (_image != null)
        {
            _image.material = null;
        }
        
        if (_material != null)
        {
            if (Application.isPlaying) Destroy(_material);
            else DestroyImmediate(_material);
        }
    }

    private void Update()
    {
#if UNITY_EDITOR
        UpdateMaterial();
#endif
    }
    
    // Called when values change in Inspector
    private void OnValidate()
    {
        UpdateMaterial();
    }

    public void SetOutlineColor(Color color)
    {
        outlineColor = color;
        UpdateMaterial();
    }

    private void UpdateMaterial()
    {
        if (_material != null)
        {
            _material.SetColor("_OutlineColor", outlineColor);
            _material.SetFloat("_OutlineWidth", outlineWidth);
        }
    }
}
