using System;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    [Header("Silinecek")]
    public Transform tmp;

    [Header("İnşa edilecek bina prefabları")]
    [SerializeField] private GameObject[] buildingPrefabs;

    [Header("BuildingGridArea")]
    [SerializeField] private Transform buildingGridRoot; // 🔧 BuildingGrid referansı
    
    private GameObject currentPreview;
    private GameObject selectedPrefab;

    [Header("Ayarlar")]
    public LayerMask placementLayer;
    public Material previewMaterial;

    //Bina animasyon yönetimi ayarları(aktif tüm binaların seçimi
    private List<BuildingBounce> activeBuildings = new();
    [SerializeField]
    private float bounceInterval = 5f;

    private void Start()
    {
        StartCoroutine(BounceLoop());
    }

    void Update()
    {
        if (selectedPrefab != null)
        {
            HandlePreview();
            if (Input.GetMouseButtonDown(0))
                PlaceBuilding();
        }
    }


    private void HandlePreview()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, placementLayer))
        {
            currentPreview.transform.position = hit.point;
        }
    }

    private void PlaceBuilding()
    {
        GameObject newBuilding = Instantiate(selectedPrefab);
        newBuilding.transform.SetParent(buildingGridRoot, false); // ✅ UI hiyerarşisine ekle

        // Eğer UI layout kullanıyorsan:
        RectTransform rt = newBuilding.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.localScale = Vector3.one;
            rt.localPosition = Vector3.zero; // veya layout’a göre ayarlanır
        }

        // ✅ Bina tarzına göre özel Stil uygula
        BuildingStyle style = newBuilding.GetComponent<BuildingStyle>();
        if (style != null)
            style.ApplyStyle();

        Destroy(currentPreview);
        selectedPrefab = null;
    }


    public GameObject CreateBuildingAtAnchored(int index, Vector2 anchoredPos)
    {
        if (index < 0 || index >= buildingPrefabs.Length) return null;

        GameObject prefab = buildingPrefabs[index];
        GameObject newBuilding = Instantiate(prefab);
        RectTransform buildingRT = newBuilding.GetComponent<RectTransform>();

        if (buildingRT != null)
        {
            buildingRT.SetParent(buildingGridRoot, false);
            buildingRT.anchoredPosition = anchoredPos;
            buildingRT.localScale = Vector3.one;
        }

        //Animasyon için Prefab instantiate ettiğin her yerde şu satırı ekle:
        BuildingBounce bounce = newBuilding.GetComponent<BuildingBounce>();
        if (bounce != null)
            activeBuildings.Add(bounce);

        return newBuilding;
    }

    //Ev animasyon tetikleme fonksiyonu
    private IEnumerator BounceLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(bounceInterval);

            if (activeBuildings.Count > 0)
            {
                int index = Random.Range(0, activeBuildings.Count);
                BuildingBounce bounceTarget = activeBuildings[index];

                if (bounceTarget != null)
                    bounceTarget.BounceOnce();
            }
        }
    }

}

