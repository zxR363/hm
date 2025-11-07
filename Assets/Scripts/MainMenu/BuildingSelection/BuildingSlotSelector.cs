using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingSlotSelector : MonoBehaviour
{
    [SerializeField] private BuildingManager buildingManager;
    [SerializeField] private int buildingIndex;

    private RectTransform slotRect;
    private GameObject currentBuilding;

    private void Awake()
    {
        slotRect = GetComponent<RectTransform>();
    }

    public void OnClickPlaceBuilding()
    {
        if (currentBuilding != null)
        {
            Debug.Log("❌ Slot already occupied");
            return;
        }

        Vector2 anchoredPos = slotRect.anchoredPosition;
        currentBuilding = buildingManager.CreateBuildingAtAnchored(buildingIndex, anchoredPos);
    }

    public void ClearSlot()
    {
        if (currentBuilding != null)
        {
            Destroy(currentBuilding);
            currentBuilding = null;
        }
    }
}

