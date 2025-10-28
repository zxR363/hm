using UnityEngine;
using UnityEngine.UI;
using System.Collections;

//Character Ekranında Object Pool animasyonu için tasarlandı. Object Pool yaptıran fonksiyon

public class FallingSpriteManager : MonoBehaviour
{
    public Sprite[] fallingSprites; // Sprite array
    public RectTransform spawnArea; // Yukarıdaki referans alan
    public RectTransform targetArea; // Aşağıdaki referans alan
    public ObjectPool pool; // ObjectPool referansı
    public float spawnInterval = 0.5f;
    public float fallDuration = 1f;

    void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            SpawnFallingSprite();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

void SpawnFallingSprite()
{
    Sprite selected = fallingSprites[Random.Range(0, fallingSprites.Length)];

    GameObject go = pool.Get();
    Image img = go.GetComponent<Image>();
    img.sprite = selected;

    RectTransform rt = go.GetComponent<RectTransform>();

    // 🔧 spawnArea ve targetArea'nın dünya pozisyonlarını al
    Vector3 worldSpawnMin = spawnArea.TransformPoint(spawnArea.rect.min);
    Vector3 worldSpawnMax = spawnArea.TransformPoint(spawnArea.rect.max);
    Vector3 worldTargetMin = targetArea.TransformPoint(targetArea.rect.min);

    // 🔧 random X pozisyonu hesapla
    float randomX = Random.Range(worldSpawnMin.x, worldSpawnMax.x);

    // 🔧 UI pozisyonuna çevir
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

    IEnumerator FallAnimation(RectTransform rt, Vector2 start, Vector2 end, GameObject obj)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / fallDuration;
            rt.anchoredPosition = Vector2.Lerp(start, end, t);
            yield return null;
        }

        pool.Return(obj);
    }
}