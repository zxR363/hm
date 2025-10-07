using UnityEngine;

public class CharacterDrag : MonoBehaviour
{

    //ACIKLAMA:: Seçilen Nesneyi hareket etmesini sağlayan script
    //NOT: Background dışına çıkması engelleniyor.Belirli alanda gezmesi sağlanıyor.
    //TODO: Background String yapısından class yapısına evrilecek

    private Vector3 offset;
    private bool isDragging = false;

    private Vector2 minBounds;
    private Vector2 maxBounds;

    private float objHalfWidth;
    private float objHalfHeight;

    void Start()
    {
        // Background sınırlarını al
        BoxCollider2D bgCollider = GameObject.Find("Background").GetComponent<BoxCollider2D>();
        Bounds bounds = bgCollider.bounds;
        minBounds = bounds.min;
        maxBounds = bounds.max;

        // Karakterin boyutunu al
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            objHalfWidth = sr.bounds.size.x / 2f;
            objHalfHeight = sr.bounds.size.y / 2f;
        }
    }

    void OnMouseDown()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        offset = transform.position - new Vector3(mouseWorldPos.x, mouseWorldPos.y, transform.position.z);
        isDragging = true;

        // Kamera hedefini bu karakter olarak ayarla
        CameraFollowing cameraFollow = Camera.main.GetComponent<CameraFollowing>();
        if (cameraFollow != null)
        {
            cameraFollow.SetTarget(transform);
        }
    }

    void OnMouseUp()
    {
        isDragging = false;
    }

    void Update()
    {
        if (isDragging)
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 newPos = new Vector3(mouseWorldPos.x, mouseWorldPos.y, transform.position.z) + offset;

            // Pozisyonu sınırla
            float clampedX = Mathf.Clamp(newPos.x, minBounds.x + objHalfWidth, maxBounds.x - objHalfWidth);
            float clampedY = Mathf.Clamp(newPos.y, minBounds.y + objHalfHeight, maxBounds.y - objHalfHeight);

            transform.position = new Vector3(clampedX, clampedY, transform.position.z);
        }
    }
}
