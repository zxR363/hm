using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StablePositionOnce : MonoBehaviour 
{
    [Header("Settings")]
    public RectTransform background; 
    public bool useBakedNormalizedPos = true;
    
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
        // Canvas.ForceUpdateCanvases();

        if (background != null && bakedBgSize.x > 0 && bakedBgSize.y > 0)
        {
            // 1. Pozisyonu Güncelle (Sadece checkbox işaretliyse)
            if (useBakedNormalizedPos)
            {
                // Background üzerindeki normalize edilmiş (0-1 arası) noktayı tekrar dünya koordinatına çevir
                Vector2 targetLocalPoint = Rect.NormalizedToPoint(background.rect, bakedNormalizedPos);
                Vector3 targetWorldPos = background.TransformPoint(targetLocalPoint);
                rt.position = targetWorldPos;
            }

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

    [ContextMenu("SYSTEMATIC FIX: Convert To Native Anchors")]
    public void ConvertToNativeAnchors()
    {
        if (background == null)
        {
            Debug.LogError("[StablePosition] Background atanmamış! Dönüştürme yapılamaz.");
            return;
        }

        rt = GetComponent<RectTransform>();

        // 1. Ensure Parenting (Optional but recommended for Anchors)
        // If the object is not a child of background, Anchors won't work relative to background automatically.
        if (transform.parent != background.transform)
        {
            Debug.LogWarning($"[StablePosition] '{name}' objesi '{background.name}' objesinin çocuğu değil! Anchor mantığı için parent olması gerekir. Lütfen hiyerarşiyi kontrol edin.");
            // We don't auto-reparent to avoid breaking other logic, but we warn.
            // If they are strictly visual, user should reparent.
        }

        // 2. Calculate Current World Corners of the Object
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);
        // corners[0] = bottom-left, corners[2] = top-right

        // 3. Convert World Corners to Background's Local Space (Normalized)
        Vector2 minLocal = background.InverseTransformPoint(corners[0]);
        Vector2 maxLocal = background.InverseTransformPoint(corners[2]);

        // 4. Normalize (0..1) relative to Background Size
        Vector2 minAnchor = Rect.PointToNormalized(background.rect, minLocal);
        Vector2 maxAnchor = Rect.PointToNormalized(background.rect, maxLocal);

        // 5. Apply Anchors
        // Undo manager allows Ctrl+Z
#if UNITY_EDITOR
        UnityEditor.Undo.RecordObject(rt, "Convert To Anchors");
#endif
        rt.anchorMin = minAnchor;
        rt.anchorMax = maxAnchor;
        
        // 6. Zero out Offsets (Make it stick to anchors)
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Debug.Log($"[StablePosition] '{name}' Native Anchor sistemine dönüştürüldü! Artık bu scripte ihtiyacınız yok.");
        
        // Optional: Disable script
        this.enabled = false;
    }
}
