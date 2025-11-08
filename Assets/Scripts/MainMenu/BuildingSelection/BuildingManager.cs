using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class BuildingManager : MonoBehaviour
{
    [Header("Template Slot Alanları")]
    [SerializeField] private List<Transform> templateSlotsAreas; // dışarıdan verilecek

    private List<GameObject> buildingPrefabs = new();
    private List<Transform> buildingGridRoots = new();

    [Header("Silme On/Off butonu")]
    [SerializeField] private Sprite deleteOffSprite;
    [SerializeField] private Sprite deleteOnSprite;
    [SerializeField] private Image deleteButtonImage;

    private bool isDeleteModeActive = false;
    private List<BuildingBounce> activeBuildings = new();
    [SerializeField] private float bounceInterval = 5f;

    private void Start()
    {
        // ExtractBuildingGridsAndPrefabs();
        // StartCoroutine(BounceLoop());
        InitializeFromTemplates(templateSlotsAreas);
    }

    private void ExtractBuildingGridsAndPrefabs()
    {
        foreach (Transform template in templateSlotsAreas)
        {
            Transform buildingGrid = template.Find("BuildingGrid");
            if (buildingGrid != null)
            {
                buildingGridRoots.Add(buildingGrid);

                foreach (Transform child in buildingGrid)
                {
                    if (child.GetComponent<IBuildingSlots>() != null)
                    {
                        buildingPrefabs.Add(child.gameObject);
                    }
                }
            }
        }

        Debug.Log($"✅ Toplam {buildingPrefabs.Count} bina slotu bulundu.");
    }

    public void ToggleDeleteButtonsMode()
    {
        if (!HasAnyBuiltSlot() && isDeleteModeActive == false)
        {
            Debug.Log("❌ Silme modu aktif edilemez: hiç bina yapılmamış.");
            return;
        }

        isDeleteModeActive = !isDeleteModeActive;

        foreach (Transform grid in buildingGridRoots)
        {
            foreach (Transform child in grid)
            {
                BuildingSlotSelector slot = child.GetComponent<BuildingSlotSelector>();
                if (slot != null)
                {
                    slot.SetDeleteMode(isDeleteModeActive);

                    if (isDeleteModeActive && slot.HasBuilding())
                        slot.ShowDeleteButton();
                    else
                        slot.HideDeleteButton();
                }
            }
        }

        if (deleteButtonImage != null)
            deleteButtonImage.sprite = isDeleteModeActive ? deleteOnSprite : deleteOffSprite;
    }

    private bool HasAnyBuiltSlot()
    {
        foreach (Transform grid in buildingGridRoots)
        {
            foreach (Transform child in grid)
            {
                BuildingSlotSelector slot = child.GetComponent<BuildingSlotSelector>();
                if (slot != null && slot.HasBuilding())
                    return true;
            }
        }
        return false;
    }

    public void buildingAnimation(int buildingIndex, GameObject buildingObj)
    {
        BuildingBounce bounce = buildingObj.GetComponent<BuildingBounce>();
        if (bounce != null)
            activeBuildings.Add(bounce);
    }

    public void RemoveBounceTarget(GameObject buildingObj)
    {
        BuildingBounce bounce = buildingObj.GetComponent<BuildingBounce>();
        if (bounce != null && activeBuildings.Contains(bounce))
        {
            activeBuildings.Remove(bounce);
        }
    }

    private IEnumerator BounceLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(bounceInterval);

            if (activeBuildings.Count > 0)
            {
                int index = Random.Range(0, activeBuildings.Count);
                BuildingBounce bounceTarget = activeBuildings[index];

                // ✅ GameObject aktif mi kontrol et
                if (bounceTarget != null && bounceTarget.gameObject.activeInHierarchy)
                {
                    bounceTarget.BounceOnce();
                }
            }
        }

    }

    public void InitializeFromTemplates(List<Transform> templateSlotsAreas)
    {
        buildingPrefabs.Clear();
        buildingGridRoots.Clear();

        foreach (Transform template in templateSlotsAreas)
        {
            Transform buildingGrid = template.Find("BuildingGrid");
            if (buildingGrid != null)
            {
                buildingGridRoots.Add(buildingGrid);

                foreach (Transform child in buildingGrid)
                {
                    if (child.GetComponent<IBuildingSlots>() != null)
                    {
                        buildingPrefabs.Add(child.gameObject);
                    }
                }
            }
        }
        StartCoroutine(BounceLoop());
        Debug.Log($"✅ BuildingManager initialized with {buildingPrefabs.Count} building slots.");
    }

}