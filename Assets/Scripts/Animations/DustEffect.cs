using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class DustEffect : MonoBehaviour
{
    [Header("Prefab Ayarları")]
    [SerializeField] private GameObject dustPrefab;
    [SerializeField] private Transform container;   // Canvas/Panel (Maskelenmemesi için)

    [Header("Hedef Ayarları")]
    [SerializeField] private Image targetImage; // Spawn merkezi olacak resim (Boşsa parent'tan bulur)

    [Header("Animasyon Ayarları")]
    [SerializeField] private int particleCount = 5;
    [SerializeField] private float riseHeight = 150f; // Alive süresi boyunca ne kadar yükselecek
    [SerializeField] private float aliveTime = 1.0f;  // Ne kadar sürecek
    [SerializeField] private float scatterAmount = 30f;

    [Header("Zamanlama Ayarları")]
    [SerializeField] private float spawnDuration = 0.5f; // Tüm partiküllerin çıkması için geçen toplam süre

    [Header("Test")]
    [SerializeField] private bool playOnStart = false;

    private void Start()
    {
        if (playOnStart)
        {
            PlayAnimation();
        }
    }

    public void PlayAnimation(Transform targetOverride = null)
    {
        if (dustPrefab == null)
        {
            Debug.LogError("DustEffect: Dust Prefab atanmamış!");
            return;
        }

        StartCoroutine(SpawnRoutine(targetOverride));
    }

    private System.Collections.IEnumerator SpawnRoutine(Transform targetOverride)
    {
        // Hedef belirle: Override > Inspector > Auto-Find
        Transform finalTarget = null;

        if (targetOverride != null)
        {
            finalTarget = targetOverride;
        }
        else if (targetImage != null)
        {
            finalTarget = targetImage.transform;
        }
        else if (transform.parent != null)
        {
            // Otomatik bulma (Fallback)
            Transform siblingImage = transform.parent.Find("Image");
            if (siblingImage != null) finalTarget = siblingImage;
            else finalTarget = transform.parent.GetComponentInChildren<Image>()?.transform;
        }

        Vector3 spawnCenter = transform.position;
        if (finalTarget != null)
        {
            // DustEffect objesini hedefin üzerine taşı (Kendi Image'inin doğru yerde çıkması için)
            transform.position = finalTarget.position;
            spawnCenter = finalTarget.position;
        }

        Transform parent = container != null ? container : transform.root;
        
        // Her bir partikül arasındaki bekleme süresi
        float interval = particleCount > 1 ? spawnDuration / (particleCount - 1) : 0f;

        for (int i = 0; i < particleCount; i++)
        {
            SpawnParticle(spawnCenter, parent);
            
            if (i < particleCount - 1)
                yield return new WaitForSeconds(interval);
        }
    }

    public float GetTotalDuration()
    {
        return spawnDuration + aliveTime;
    }

    private void SpawnParticle(Vector3 centerPos, Transform parent)
    {
        // 1. Oluştur
        GameObject particle = Instantiate(dustPrefab, parent);
        particle.SetActive(true);
        
        // Rastgele başlangıç pozisyonu (Merkezin etrafında ufak dağılım)
        float startX = Random.Range(-15f, 15f);
        float startY = Random.Range(-15f, 15f);
        particle.transform.position = centerPos + new Vector3(startX, startY, 0);
        
        // Scale reset
        particle.transform.localScale = Vector3.one * Random.Range(0.8f, 1.2f);

        // 2. Animasyon Sequence
        Sequence seq = DOTween.Sequence();
        
        // Hedef Y (Alive süresi boyunca çıkacağı yükseklik)
        float targetY = particle.transform.position.y + riseHeight;
        
        // Hedef X (Sağa sola savrulma)
        float targetX = particle.transform.position.x + Random.Range(-scatterAmount, scatterAmount);
        
        // Yükselme ve Dağılma
        seq.Join(particle.transform.DOMoveY(targetY, aliveTime).SetEase(Ease.OutSine));
        seq.Join(particle.transform.DOMoveX(targetX, aliveTime).SetEase(Ease.OutSine));

        // Fade Out (Sonlara doğru)
        Image img = particle.GetComponent<Image>();
        if (img != null)
        {
            Color c = img.color;
            c.a = 1f;
            img.color = c;
            
            // Sürenin yarısından sonra silinmeye başla
            seq.Join(img.DOFade(0f, aliveTime * 0.6f).SetDelay(aliveTime * 0.4f).SetEase(Ease.InQuad));
        }

        // 3. Yok Et
        seq.OnComplete(() => {
            Destroy(particle);
        });
    }
}
