using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// Character ekranında object pool ile düşen sprite animasyonu
public class FallingSpriteManager : MonoBehaviour
{
    [Header("Sprite Havuzu")]
    public Sprite[] fallingSprites;

    [Header("Alanlar")]
    public RectTransform spawnArea;
    public RectTransform targetArea;

    [Header("Object Pool")]
    public ObjectPool pool;

    [Header("Zamanlama")]
    public float spawnInterval = 0.5f;
    public float fallDuration = 1f;

    private Coroutine spawnRoutine;

    void Start()
    {
        TriggerAnimations();
    }

    public void TriggerAnimations()
    {
        if (fallingSprites == null || fallingSprites.Length == 0 || spawnArea == null || targetArea == null || pool == null)
        {
            Debug.LogWarning($"[{name}] FallingSpriteManager: Eksik referanslar.");
            return;
        }

        if (spawnRoutine != null)
            StopCoroutine(spawnRoutine);

        spawnRoutine = StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            SpawnFallingSprite();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnFallingSprite()
    {
        Sprite selected = fallingSprites[Random.Range(0, fallingSprites.Length)];

        GameObject go = pool.Get();
        Image img = go.GetComponent<Image>();
        img.sprite = selected;
        img.enabled = true;

        RectTransform rt = go.GetComponent<RectTransform>();

        // Dünya pozisyonlarını al
        Vector3 worldSpawnMin = spawnArea.TransformPoint(spawnArea.rect.min);
        Vector3 worldSpawnMax = spawnArea.TransformPoint(spawnArea.rect.max);
        Vector3 worldTargetMin = targetArea.TransformPoint(targetArea.rect.min);

        float randomX = Random.Range(worldSpawnMin.x, worldSpawnMax.x);

        // UI local pozisyonuna çevir
        Vector2 localSpawnPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rt.parent as RectTransform,
            new Vector2(randomX, worldSpawnMax.y),
            null,
            out localSpawnPos
        );

        Vector2 localTargetPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rt.parent as RectTransform,
            new Vector2(randomX, worldTargetMin.y),
            null,
            out localTargetPos
        );

        rt.anchoredPosition = localSpawnPos;

        StartCoroutine(FallAnimation(rt, localSpawnPos, localTargetPos, go));
    }

    private IEnumerator FallAnimation(RectTransform rt, Vector2 start, Vector2 end, GameObject obj)
    {
        if (pool == null)
        {
            Debug.LogWarning("Pool referansı null, Return çağrısı yapılamıyor.");
            yield break;
        }

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / fallDuration;
            rt.anchoredPosition = Vector2.Lerp(start, end, t);
            yield return null;
        }

        pool.Return(obj);
    }

    private void OnDisable()
    {
        if (spawnRoutine != null)
            StopCoroutine(spawnRoutine);
    }
}