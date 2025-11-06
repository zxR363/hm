using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
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

        currentPreview = Instantiate(selectedPrefab);
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
        GameObject newBuilding = Instantiate(selectedPrefab, currentPreview.transform.position, Quaternion.identity);
        newBuilding.transform.SetParent(buildingGridRoot, false); // ✅ prefab BuildingGrid altında

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
}

