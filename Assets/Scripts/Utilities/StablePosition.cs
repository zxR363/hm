using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StablePositionOnce : MonoBehaviour 
{
    [Header("Settings")]
    public RectTransform background; 
    
    [Header("Baked Data (Right Click -> Bake Position)")]
    [SerializeField] private Vector2 bakedNormalizedPos;
    [SerializeField] private Vector3 bakedLocalScale;
    [SerializeField] private Vector2 bakedBgSize; // Bake anındaki background boyutu (Scale oranı için)

    private RectTransform rt;

    private void Awake()
    {
        rt = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        StartCoroutine(ApplyCorrection());
    }

    private IEnumerator ApplyCorrection()
    {
        yield return null; // Layout hesaplamaları için bekle
        Canvas.ForceUpdateCanvases();

        if (background != null && bakedBgSize.x > 0 && bakedBgSize.y > 0)
        {
            // 1. Pozisyonu Güncelle
            // Background üzerindeki normalize edilmiş (0-1 arası) noktayı tekrar dünya koordinatına çevir
            Vector2 targetLocalPoint = Rect.NormalizedToPoint(background.rect, bakedNormalizedPos);
            Vector3 targetWorldPos = background.TransformPoint(targetLocalPoint);
            rt.position = targetWorldPos;

            // 2. Scale'i Güncelle
            // Background'ın ne kadar büyüdüğünü hesapla
            float ratioX = background.rect.width / bakedBgSize.x;
            float ratioY = background.rect.height / bakedBgSize.y;

            // Scale oranını uygula
            rt.localScale = new Vector3(bakedLocalScale.x * ratioX, bakedLocalScale.y * ratioY, bakedLocalScale.z);
        }
        
        enabled = false;
    }

    [ContextMenu("Bake Position & Scale")]
    public void BakePosition()
    {
        if (background == null)
        {
            Debug.LogError("Lütfen önce Background objesini atayın!");
            return;
        }

        rt = GetComponent<RectTransform>();
        
        // Objenin background içindeki local pozisyonunu bul
        Vector3 localPosInBg = background.InverseTransformPoint(rt.position);
        
        // Bu pozisyonu normalize et (0,0 sol alt, 1,1 sağ üst)
        bakedNormalizedPos = Rect.PointToNormalized(background.rect, localPosInBg);
        
        // Şu anki scale ve background boyutunu kaydet
        bakedLocalScale = rt.localScale;
        bakedBgSize = background.rect.size;

        Debug.Log($"Pozisyon ve Scale kaydedildi! Normalized Pos: {bakedNormalizedPos}, Bg Size: {bakedBgSize}");
    }
}
