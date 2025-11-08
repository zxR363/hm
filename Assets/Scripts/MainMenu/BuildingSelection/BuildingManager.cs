using System;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using UnityEngine.UI;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    [Header("İnşa edilecek bina prefabları")]
    [SerializeField] private GameObject[] buildingPrefabs;

    [Header("BuildingGridArea")]
    [SerializeField] private Transform buildingGridRoot; // 🔧 BuildingGrid referansı
    
    private GameObject currentPreview;
    private GameObject selectedPrefab;

    [Header("Silme On/Off butonu")]
    [SerializeField] private Sprite deleteOffSprite; // normal hali (boş çöp kutusu)
    [SerializeField] private Sprite deleteOnSprite;  // aktif hali (dolu/kırmızı çöp kutusu)
    [SerializeField] private Image deleteButtonImage; // butonun içindeki Image referansı

    
    private List<BuildingSlotSelector> allSlots;

    private bool isDeleteModeActive = false;

    //Bina animasyon yönetimi ayarları(aktif tüm binaların seçimi)
    private List<BuildingBounce> activeBuildings = new();
    [SerializeField]
    private float bounceInterval = 5f;
    //Bina animasyon yönetimi ayarları(aktif tüm binaların seçimi)

    private void Start()
    {
        StartCoroutine(BounceLoop());
    }

    void Update()
    {

    }

    public void ToggleDeleteButtonsMode()
    {

        // 🔒 En az 1 bina yapılmış mı?
        if (!HasAnyBuiltSlot() && isDeleteModeActive == false)
        {
            Debug.Log("❌ Silme modu aktif edilemez: hiç bina yapılmamış.");
            return;
        }

        isDeleteModeActive = !isDeleteModeActive;

        foreach (Transform child in buildingGridRoot)
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
        
        // 🔁 Sprite değişimi
        if (deleteButtonImage != null)
            deleteButtonImage.sprite = isDeleteModeActive ? deleteOnSprite : deleteOffSprite;

    }

    //Ekranda herhangi bir mevcut Building(bina) var mı kontrol ediyor
    private bool HasAnyBuiltSlot()
    {
        foreach (Transform child in buildingGridRoot)
        {
            BuildingSlotSelector slot = child.GetComponent<BuildingSlotSelector>();
            if (slot != null && slot.HasBuilding())
                return true;
        }
        return false;
    }

    //Animasyon için Prefab instantiate ettiğin her yerde şu satırı ekle:
    public void buildingAnimation(int buildingIndex, GameObject buildingObj)
    {
        
        BuildingBounce bounce = buildingObj.GetComponent<BuildingBounce>();
        if (bounce != null)
            activeBuildings.Add(bounce);
    }

    //Animasyonu kaldırma
    public void RemoveBounceTarget(GameObject buildingObj)
    {
        BuildingBounce bounce = buildingObj.GetComponent<BuildingBounce>();
        if (bounce != null && activeBuildings.Contains(bounce))
        {
            activeBuildings.Remove(bounce);
        }
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

