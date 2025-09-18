using UnityEngine;

public class CameraFollowing : MonoBehaviour
{
    //ACIKLAMA:: Seçilen Nesneyi kameranın takip etmesine yarayan script

    [Header("Takip Ayarları")]
    public Transform target;
    public Vector3 offset = new Vector3(0, 0, -10);
    [Range(0.001f, 1f)] public float smoothSpeed = 0.125f;

    [Header("Sınırlar")]
    public BoxCollider2D boundary;

    private float camHalfHeight;
    private float camHalfWidth;
    private Vector2 minBounds;
    private Vector2 maxBounds;

    void Start()
    {
        Camera cam = Camera.main;
        camHalfHeight = cam.orthographicSize;
        camHalfWidth = cam.aspect * camHalfHeight;

        if (boundary != null)
        {
            Bounds bounds = boundary.bounds;
            minBounds = bounds.min;
            maxBounds = bounds.max;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;

        // Sınırları uygula
        float clampedX = Mathf.Clamp(desiredPosition.x, minBounds.x + camHalfWidth, maxBounds.x - camHalfWidth);
        float clampedY = Mathf.Clamp(desiredPosition.y, minBounds.y + camHalfHeight, maxBounds.y - camHalfHeight);

        Vector3 clampedPosition = new Vector3(clampedX, clampedY, transform.position.z);

        // Yumuşak takip
        transform.position = Vector3.Lerp(transform.position, clampedPosition, smoothSpeed);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

}
