using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Collections;

public class BuildingSlotSelector : MonoBehaviour
{
    [Header("Bağlantılar")]
    [SerializeField] private BuildingManager buildingManager;
    [SerializeField] private int buildingIndex;

    [Header("Efektler")]
    [SerializeField] private DustEffect dustEffect;

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
    private bool isAnimating = false;

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
        if (slotVisual == null || isAnimating) return;

        if (isDeleteMode)
        {
            if (isBuilt && deleteButtonUI != null)
                deleteButtonUI.SetActive(true);
            return;
        }

        if (!isBuilt)
        {
            StartCoroutine(BuildSequence());
        }
        else
        {
            SceneLoader.LoadSceneWithTransition(sceneName);
        }
    }

    private IEnumerator BuildSequence()
    {
        isAnimating = true;

        Image dustImage = null;
        if (dustEffect != null)
        {
            dustImage = dustEffect.GetComponent<Image>();
        }

        // 1. Görsel Geçişi Başlat
        if (slotVisual != null) slotVisual.gameObject.SetActive(false); // Ana görseli gizle
        if (dustImage != null) dustImage.enabled = true; // Dust görselini aç

        // 2. Toz Efektini Oynat
        if (dustEffect != null)
        {
            // SlotVisual'ın olduğu yerden başlat
            dustEffect.PlayAnimation(slotVisual != null ? slotVisual.transform : null);
            
            // Efekt bitene kadar bekle
            yield return new WaitForSeconds(dustEffect.GetTotalDuration() * 0.8f); 
        }

        // 3. Binayı İnşa Et (Görseli değiştir)
        isBuilt = true;
        SetVisualBuilt();
        
        // 4. Görselleri Geri Yükle
        if (slotVisual != null) slotVisual.gameObject.SetActive(true); // Ana görseli aç
        if (dustImage != null) dustImage.enabled = false; // Dust görselini kapat
        
        // 5. Diğer Animasyonlar (Bounce vb.)
        buildingManager.buildingAnimation(buildingIndex, transform.gameObject);
        BuildingSaveSystem.SaveSlotState(slotID, true);

        isAnimating = false;
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
        Image hummerVisual = slotVisual.transform.Find("hummer")?.GetComponent<Image>();
        if (hummerVisual != null) hummerVisual.gameObject.SetActive(false);
    }

    private void SetVisualEmpty()
    {
        IBuildingSlots slotData = GetComponent<IBuildingSlots>();
        if (slotData == null || slotVisual == null) return;

        slotVisual.sprite = emptySprite;
        slotVisual.rectTransform.localPosition = slotData.emptyBuildingPosition;
        slotVisual.rectTransform.sizeDelta = slotData.emptyBuildingSize;
        slotVisual.rectTransform.localEulerAngles = slotData.emptyBuildingRotation;
        Image hummerVisual = slotVisual.transform.Find("hummer")?.GetComponent<Image>();
        if (hummerVisual != null) hummerVisual.gameObject.SetActive(true);
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