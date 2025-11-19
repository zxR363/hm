using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;


public class BuildingSlotSelector : MonoBehaviour
{
    [Header("Bağlantılar")]
    [SerializeField] private BuildingManager buildingManager;
    [SerializeField] private int buildingIndex;

    [Header("Görsel Ayarları")]
    [SerializeField] private Sprite emptySprite;
    [SerializeField] private Sprite buildingSprite;
    [SerializeField] private string sceneName;

    [Header("Boyut ve Dönüş Ayarları")]
    [SerializeField] private Vector2 emptySize = new Vector2(100f, 100f);
    [SerializeField] private Vector2 buildingSize = new Vector2(150f, 150f);
    [SerializeField] private Vector3 emptyRotation = new Vector3(60.4f, 33f, 0f);
    [SerializeField] private Vector3 buildingRotation = new Vector3(0f, 0f, -5.01f);

    [Header("Durum")]
    public bool isDeleteMode = false;

    private RectTransform slotRect;
    private Image slotVisual;
    private GameObject deleteButtonUI;
    private bool isBuilt = false;
    private string slotID;

    private void Awake()
    {
        slotID = gameObject.name;

        slotRect = GetComponent<RectTransform>();
        slotVisual = transform.Find("Image")?.GetComponent<Image>();
        deleteButtonUI = transform.Find("Delete")?.gameObject;

        if (deleteButtonUI != null)
            deleteButtonUI.SetActive(false);

        // 🎯 Kayıtlı durumu yükle
        if (BuildingSaveSystem.TryGetSlotState(slotID, out bool wasBuilt) && wasBuilt)
        {
            isBuilt = true;
            SetVisualBuilt();

            // ✅ Eğer altında "EmptyBuilding" içeren bir obje yoksa bounce ekle
            if (!HasEmptyBuildingChild())
            {
                // ✅ Bounce hedefi ekle
                buildingManager.buildingAnimation(buildingIndex, transform.gameObject);

                // ✅ İsteğe bağlı: sahne açılışında bir kez wobble
                BuildingBounce bounce = GetComponent<BuildingBounce>();
                if (bounce != null)
                    bounce.BounceOnce();
            }
        }
        else
        {
            isBuilt = false;
            SetVisualEmpty();
        }
    }

    public void OnClickSlot()
    {
        if (slotVisual == null) return;

        if (isDeleteMode)
        {
            if (isBuilt && deleteButtonUI != null)
                deleteButtonUI.SetActive(true);
            return;
        }

        if (!isBuilt)
        {
            isBuilt = true;
            SetVisualBuilt();
            buildingManager.buildingAnimation(buildingIndex, transform.gameObject);
            BuildingSaveSystem.SaveSlotState(slotID, true); // ✅ kayıt
        }
        else
        {
            SceneLoader.LoadSceneWithTransition(sceneName);
        }
    }

    public void resetBuildingSlot()
    {
        if (!isDeleteMode)
        {
            Debug.LogWarning("❌ Silme modu kapalı, slot sıfırlanamaz.");
            return;
        }

        if (isBuilt)
        {
            isBuilt = false;

            if (buildingManager != null && slotVisual != null)
                buildingManager.RemoveBounceTarget(transform.gameObject);

            SetVisualEmpty();

            if (deleteButtonUI != null)
                deleteButtonUI.SetActive(false);

            BuildingSaveSystem.SaveSlotState(slotID, false); // ✅ silme kaydı
        }
    }

    public bool HasBuilding()
    {
        return isBuilt;
    }

    public void SetDeleteMode(bool active)
    {
        isDeleteMode = active;
    }

    public void ShowDeleteButton()
    {
        if (deleteButtonUI != null)
            deleteButtonUI.SetActive(true);

    }

    public void HideDeleteButton()
    {
        if (deleteButtonUI != null)
            deleteButtonUI.SetActive(false);
    }

    private void SetVisualBuilt()
    {
        IBuildingSlots slotData = GetComponent<IBuildingSlots>();

        if (slotData == null || slotVisual == null) return;

        slotVisual.sprite = buildingSprite;
        slotVisual.rectTransform.localPosition = slotData.builtBuildingPosition;
        slotVisual.rectTransform.sizeDelta = slotData.builtBuildingSize;
        slotVisual.rectTransform.localEulerAngles = slotData.builtBuildingRotation;
    }

    private void SetVisualEmpty()
    {
        IBuildingSlots slotData = GetComponent<IBuildingSlots>();
        if (slotData == null || slotVisual == null) return;

        slotVisual.sprite = emptySprite;
        slotVisual.rectTransform.localPosition = slotData.emptyBuildingPosition;
        slotVisual.rectTransform.sizeDelta = slotData.emptyBuildingSize;
        slotVisual.rectTransform.localEulerAngles = slotData.emptyBuildingRotation;
    }

    //Building Alanı Boş mu Dolu mu diye kontrol ediyor
    private bool HasEmptyBuildingChild()
    {
        foreach (Transform child in transform)
        {
            if (child.name.Contains("EmptyBuilding", StringComparison.OrdinalIgnoreCase))
                return true;

            // Altında başka çocuklar varsa onları da kontrol et
            foreach (Transform grandChild in child)
            {
                if (grandChild.name.Contains("EmptyBuilding", StringComparison.OrdinalIgnoreCase))
                    return true;
            }
        }

        return false;
    }
}