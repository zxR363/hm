using UnityEngine;

public class TemplateDistributor : MonoBehaviour
{
    [Header("Template Ayarları")]
    [SerializeField] private Transform[] templateAreas;
    [SerializeField] private float sphereRadius = 10f;

    private void Start()
    {
        int count = templateAreas.Length;

        for (int i = 0; i < count; i++)
        {
            float theta = (360f / count) * i;      // yatay açı
            float phi = 90f;                       // sabit dikey açı (ekvator)

            Vector3 pos = GetSpherePosition(sphereRadius, theta, phi);
            templateAreas[i].localPosition = pos;
            templateAreas[i].LookAt(Vector3.zero); // merkeze dönük olsun
        }
    }

    private Vector3 GetSpherePosition(float radius, float thetaDeg, float phiDeg)
    {
        float theta = Mathf.Deg2Rad * thetaDeg;
        float phi = Mathf.Deg2Rad * phiDeg;

        float x = radius * Mathf.Sin(phi) * Mathf.Cos(theta);
        float y = radius * Mathf.Cos(phi);
        float z = radius * Mathf.Sin(phi) * Mathf.Sin(theta);

        return new Vector3(x, y, z);
    }
}
