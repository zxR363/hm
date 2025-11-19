using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
    [SerializeField] private Vector3 emptyRotation = new Vector3(64.651f, 17.242f, -6.166f);
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
        if (slotVisual != null)
        {
            slotVisual.sprite = buildingSprite;
            slotVisual.rectTransform.sizeDelta = buildingSize;
            slotVisual.rectTransform.localRotation = Quaternion.Euler(buildingRotation);
        }
    }

    private void SetVisualEmpty()
    {
        if (slotVisual != null)
        {
            slotVisual.sprite = emptySprite;
            slotVisual.rectTransform.sizeDelta = emptySize;
            slotVisual.rectTransform.localRotation = Quaternion.Euler(emptyRotation);
        }
    }
}