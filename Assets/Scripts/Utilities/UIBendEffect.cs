using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

//SALINIM ICIN EFEKT(JÖLE GİBİ OLAN)

[RequireComponent(typeof(RectTransform))]
public class UIBendEffect : BaseMeshEffect
{
    [Range(-1f, 1f)]
    public float bendAmount = 0f;

    [Tooltip("Eğilmenin üssü (1 = Lineer, >1 = Exponensiyal)")]
    public float exponent = 2f;

    private Graphic _graphic;
    private RectTransform _rectTransform;

    protected override void Awake()
    {
        base.Awake();
        _graphic = GetComponent<Graphic>();
        _rectTransform = GetComponent<RectTransform>();
    }

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive() || vh.currentVertCount == 0)
            return;

        List<UIVertex> vertices = new List<UIVertex>();
        vh.GetUIVertexStream(vertices);

        if (_rectTransform == null) _rectTransform = GetComponent<RectTransform>();
        
        float height = _rectTransform.rect.height;
        float bottomY = _rectTransform.rect.yMin;

        // Debug.Log($"ModifyMesh Running. Verts: {vertices.Count}, Bend: {bendAmount}");

        for (int i = 0; i < vertices.Count; i++)
        {
            UIVertex v = vertices[i];
            Vector3 pos = v.position;

            // 0 (alt) ile 1 (üst) arasında normalize edilmiş Y değeri
            float normalizedY = Mathf.InverseLerp(bottomY, bottomY + height, pos.y);

            // Exponensiyal etki
            float effectFactor = Mathf.Pow(normalizedY, exponent);

            // X ekseninde kaydırma
            pos.x += bendAmount * height * effectFactor;

            v.position = pos;
            vertices[i] = v;
        }

        vh.Clear();
        vh.AddUIVertexTriangleStream(vertices);
    }

    public void SetBend(float amount)
    {
        bendAmount = amount;
        
        if (_graphic == null) _graphic = GetComponent<Graphic>();
        
        if (_graphic != null)
        {
            _graphic.SetVerticesDirty();
        }
    }
}
