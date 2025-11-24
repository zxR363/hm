using UnityEngine;

public class AirplaneMovementBehaviour : MonoBehaviour
{
    [Header("Orbit Settings")]
    public Transform centerPoint;
    public float orbitSpeed = 20f; // derece/saniye
    public bool clockwise = true;

    private float radius;
    private float angle;
    private float initialOffset; // Başlangıç sprite açısı ile merkeze bakış farkı

    void Start()
    {
        if (centerPoint == null)
        {
            Debug.LogError("Center point atanmadı!");
            return;
        }

        // Başlangıç offset vektörü ve radius
        Vector3 offset = transform.position - centerPoint.position;
        radius = offset.magnitude;
        angle = Mathf.Atan2(offset.y, offset.x);

        // Başlangıçta merkeze bakacak açı
        float centerAngle = Mathf.Atan2(-offset.y, -offset.x) * Mathf.Rad2Deg;

        // Başlangıç offset’ini DeltaAngle ile al → normalize edilmiş
        initialOffset = Mathf.DeltaAngle(centerAngle, transform.eulerAngles.z);
    }

    void Update()
    {
        // Orbit açısını güncelle
        angle += (clockwise ? -1f : 1f) * orbitSpeed * Mathf.Deg2Rad * Time.deltaTime;

        // Yeni pozisyon
        float x = Mathf.Cos(angle) * radius;
        float y = Mathf.Sin(angle) * radius;
        Vector3 newPos = centerPoint.position + new Vector3(x, y, 0);
        transform.position = newPos;

        // Merkeze bakan açı
        Vector3 dirToCenter = centerPoint.position - transform.position;
        float centerAngle = Mathf.Atan2(dirToCenter.y, dirToCenter.x) * Mathf.Rad2Deg;

        // DeltaAngle ile başlangıç offset’i uygula → açısal değişim doğal olur
        float finalAngle = Mathf.DeltaAngle(centerAngle, centerAngle + initialOffset) + centerAngle;

        transform.rotation = Quaternion.Euler(0, 0, finalAngle);
    }
}