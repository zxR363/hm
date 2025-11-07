using System;
using System.Collections;
using System.Collections.Generic;
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

    void Update()
    {
        if (selectedPrefab != null)
        {
            HandlePreview();
            if (Input.GetMouseButtonDown(0))
                PlaceBuilding();
        }
    }

    public void SelectBuilding(int index)
    {
        if (index < 0 || index >= buildingPrefabs.Length) return;

        selectedPrefab = buildingPrefabs[index];
        CreatePreview();
    }

    private void CreatePreview()
    {
        if (currentPreview != null)
            Destroy(currentPreview);

        //currentPreview = Instantiate(selectedPrefab);

        Debug.Log("Butonun=" + selectedPrefab.transform.localPosition + "  " + selectedPrefab.transform.position);
        Debug.Log("DigerBTn=" + tmp.localPosition + "  " + tmp.position);

        Vector3 spawnPosition = selectedPrefab.transform.localPosition; // 🔍 Butonun pozisyonu
        CreateBuildingAt(spawnPosition);

        currentPreview = selectedPrefab;
        ApplyPreviewMaterial(currentPreview);
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

        Destroy(currentPreview);
        selectedPrefab = null;
    }


    private void ApplyPreviewMaterial(GameObject obj)
    {
        foreach (Renderer r in obj.GetComponentsInChildren<Renderer>())
        {
            r.material = previewMaterial;
        }
    }

    public void CreateBuildingAt(Vector3 position)
    {
        GameObject newBuilding = Instantiate(selectedPrefab, position, Quaternion.identity);
        newBuilding.transform.SetParent(buildingGridRoot, false);
    }


}

