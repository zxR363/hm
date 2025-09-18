using UnityEngine;

public class SpriteChanger : MonoBehaviour
{
    public Sprite newSprite; // Yeni sprite'ı buradan atayın.
    public float changeTime = 2.0f; // Sprite'nın ne saniyede değişeceğini belirleyin.

    private SpriteRenderer spriteRenderer;
    private float timer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer bulunamadı!");
        }
    }

    void Update()
    {
        if (spriteRenderer != null && newSprite != null)
        {
            timer += Time.deltaTime;

            if (timer >= changeTime)
            {
                spriteRenderer.sprite = newSprite;
            }
        }
    }
}