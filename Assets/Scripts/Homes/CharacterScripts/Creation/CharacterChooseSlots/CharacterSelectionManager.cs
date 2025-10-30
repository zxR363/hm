using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;


#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class CharacterSelectionManager : MonoBehaviour
{
    public static CharacterSelectionManager Instance;

    [Header("Managers")]
    public CharacterCreationManager characterCreationManager;

    [Header("Character Panels")]
    public GameObject characterSlotPanel;
    public GameObject characterCreationPanel;

    [Header("OptionGridPanel-Content")]
    public GameObject optionGridContent;


    [Header("Slot ve Preview")]
    public CharacterSlot selectedSlot;
    public Transform previewArea; //

    private GameObject currentPreviewInstance;

     [Header("CharacterPrefab Kaydetme")]
    public int characterCanvasSortOrder = 10; // 🔥 Prefabs sortingLayer değeri 
    public float characterScaleFactor = 0.5f; // 🔥 Prefabs scaleFactor
    public string prefabSavePath = "Assets/Resources/GeneratedCharacters/";



    void Awake()
    {
        Instance = this;
    }

    public void SelectSlot(CharacterSlot slot)
    {
        // 🔄 Panel geçişi
        characterSlotPanel.SetActive(false);
        characterCreationPanel.SetActive(true);

        //RectTransform rt = characterCreationPanel.GetComponent<RectTransform>();
        //StartCoroutine(AnimatePanelIn(rt)); // sağdan kayarak gelsin

        RectTransform panelRT = characterCreationPanel.GetComponent<RectTransform>();
        CanvasGroup cg = characterCreationPanel.GetComponent<CanvasGroup>();
        if (cg == null) cg = characterCreationPanel.gameObject.AddComponent<CanvasGroup>();

        StartCoroutine(SlideDiagonalAndFadeIn(panelRT, cg));

        selectedSlot = slot;

        ResetOptionGridToDefault();

        StartCoroutine(DelayedPreview(slot.characterInstance));
    }

    // !!!! DİKKAT: Dinamik olarak CharacterCreationPanel'deki PreviewArea'nın altına
    // ilgili prefab'ı eklemeye imkan tanıyor.
    // SetActive gibi bir durumdan kaynaklı olarak Hierarchy de gözükmüyor
    // o sebeple Coroutine ile yapıyoruz bu işlemi 1 sonraki frame de koyuyor.
    IEnumerator DelayedPreview(GameObject prefab)
    {
        characterCreationPanel.SetActive(true); // paneli aktif et

        yield return null; // bir frame bekle → Unity aktifliği işlesin

        if (!previewArea.gameObject.activeInHierarchy)
        {
            Debug.LogWarning("PreviewArea hala aktif değil!");
            yield break;
        }

        GameObject previewInstance = Instantiate(prefab);
        previewInstance.name = "CharacterPreview";

        // UI bağlama
        RectTransform rt = previewInstance.GetComponent<RectTransform>();
        rt.SetParent(previewArea, false);

        // 🔧 Pozisyon ve layout ayarları
        rt.localScale = Vector3.one;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(400, 800); // sabit boyut

        characterCreationManager.previewInstance = previewInstance;
    }

    //OptionGrid-Content içerisindeki tüm eski OptionItem'ları temizliyor.
    public void ResetOptionGridToDefault()
    {
        // 1. OptionGrid içeriğini temizle
        foreach (Transform child in optionGridContent.transform)
        {
            Destroy(child.gameObject);
        }

        // 2. Scroll pozisyonunu sıfırla
        ScrollRect scrollRect = optionGridContent.GetComponentInParent<ScrollRect>();
        if (scrollRect != null)
            scrollRect.verticalNormalizedPosition = 1f;

        // 3. Default kategoriye set et (örneğin “Skin”)
        characterCreationManager.SetCategory(0);
    }


    void PrintHierarchy(GameObject obj)
    {
        Transform current = obj.transform;
        string hierarchy = current.name;

        while (current.parent != null)
        {
            current = current.parent;
            hierarchy = current.name + " → " + hierarchy;
        }

        Debug.Log("Tam hiyerarşi: " + hierarchy);
    }

    public void ConfirmCharacter()
    {
        if (selectedSlot == null || characterCreationManager.previewInstance == null)
        {
            Debug.LogWarning("ConfirmCharacter: Slot veya preview eksik");
            return;
        }

        // 🔥 Karakteri kaydet
        SaveConfirmButtonCharacterPrefab();

        // 🔥 Preview’ı sahneden kaldır
        if (characterCreationManager.previewInstance != null && characterCreationManager.previewInstance.scene.IsValid())
        {
            Destroy(characterCreationManager.previewInstance);
        }
        else
        {
            Debug.LogWarning("SetCharacter: Asset referansı silinemez");
        }
        characterCreationManager.previewInstance = null;
    }


    //-------------PreviewArea'daki KARAKTER PREFAB KAYDETME ISLEMINI YAPIYOR------------
    public void SaveConfirmButtonCharacterPrefab()
    {
        if (characterCreationManager.previewInstance == null)
        {
            Debug.LogWarning("PreviewInstance bulunamadı");
            return;
        }

        #if UNITY_EDITOR
                // 🔥 Orijinal scale'ı sakla
                Vector3 originalScale = characterCreationManager.previewInstance.transform.localScale;

                // 🔧 Küçültme işlemi
                characterCreationManager.previewInstance.transform.localScale = originalScale * characterScaleFactor;

                // 🔧 Canvas bileşeni ekle (yoksa)
                Canvas canvas = characterCreationManager.previewInstance.GetComponent<Canvas>();
                if (canvas == null)
                    canvas = characterCreationManager.previewInstance.AddComponent<Canvas>();

                canvas.overrideSorting = true;
                canvas.sortingOrder = characterCanvasSortOrder;

                // 🔧 CanvasGroup ekle (yoksa)
                if (characterCreationManager.previewInstance.GetComponent<CanvasGroup>() == null)
                    characterCreationManager.previewInstance.AddComponent<CanvasGroup>();


                // 🔥 Prefab olarak kaydet
                //string prefabName = "Character";
                //string prefabName = characterCreationManager.previewInstance.name;
                string prefabName = selectedSlot.name;
                string fullPath = prefabSavePath + prefabName + ".prefab";

                PrefabUtility.SaveAsPrefabAsset(characterCreationManager.previewInstance, fullPath);
                Debug.Log("Karakter prefab olarak kaydedildi: " + fullPath);

                // 🔄 Scale'ı geri al (sahne içi görünüm bozulmasın)
                characterCreationManager.previewInstance.transform.localScale = originalScale;

                // 🔄 Prefab’ı tekrar yükle ve slot’a ata
                string resourcePath = "GeneratedCharacters/" + prefabName;
                Debug.Log("RSC PATH="+resourcePath);
                GameObject loadedPrefab = Resources.Load<GameObject>(resourcePath);
                if (loadedPrefab != null)
                {
                    selectedSlot.SetCharacter(loadedPrefab);
                }
                else
                {
                    Debug.LogError("Prefab yüklenemedi: " + resourcePath);
                }

        #else
                Debug.LogWarning("Prefab kaydetme sadece Editor modunda çalışır");
        #endif    

    }

    //Confirm mesajı sonrasında Paneller arası geçiş yapmamızı sağlayan fonksiyon
    //Confirm'butonunda fonksiyon olarak 
    public void ConfirmButtonPanelSwitch()
    {
        // 🔄 Panel geçişi
        characterCreationPanel.SetActive(false);
        characterSlotPanel.SetActive(true);


        // SlotPanel altındaki tüm CanvasGroup bileşenlerini topla
        CanvasGroup[] allGroups = characterSlotPanel.GetComponentsInChildren<CanvasGroup>(true);
        List<CanvasGroup> fadeTargets = new List<CanvasGroup>(allGroups);

        // Tüm GameObject’leri tarayıp eksik olanlara CanvasGroup ekle
        Transform[] allChildren = characterSlotPanel.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in allChildren)
        {
            if (child.GetComponent<CanvasGroup>() == null)
            {
                CanvasGroup cg = child.gameObject.AddComponent<CanvasGroup>();
                cg.alpha = 0;
                cg.interactable = false;
                cg.blocksRaycasts = false;
                fadeTargets.Add(cg);
            }
        }


        StartCoroutine(FadeInAllAtOnce(fadeTargets, 0.8f));

    }

    //----CharacterCreationPanel açılırken animasyon ile açılması
    public IEnumerator SlideDiagonalAndFadeIn(RectTransform panelRT, CanvasGroup cg, float duration = 0.4f)
    {
        // Başlangıç pozisyonu: sağ alt köşe
        Vector2 startPos = new Vector2(Screen.width, -Screen.height);
        // Hedef pozisyon: sol üst köşe (merkezde sabitlenmiş panel için genelde (0,0))
        Vector2 endPos = new Vector2(0, 0);

        panelRT.anchoredPosition = startPos;
        cg.alpha = 0;
        cg.interactable = false;
        cg.blocksRaycasts = false;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = 1 - Mathf.Pow(1 - t, 3); // EaseOutCubic

            panelRT.anchoredPosition = Vector2.Lerp(startPos, endPos, eased);
            cg.alpha = eased;

            yield return null;
        }

        panelRT.anchoredPosition = endPos;
        cg.alpha = 1;
        cg.interactable = true;
        cg.blocksRaycasts = true;
    }


    public IEnumerator FadeInAllAtOnce(List<CanvasGroup> groups, float duration = 1.5f)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = 1 - Mathf.Pow(1 - t, 3); // EaseOutCubic

            foreach (CanvasGroup cg in groups)
            {
                if (cg != null && cg.gameObject != null && cg.gameObject.activeInHierarchy)
                {
                    cg.alpha = eased;
                }
            }

            yield return null;
        }

        foreach (CanvasGroup cg in groups)
        {
            if (cg != null && cg.gameObject != null && cg.gameObject.activeInHierarchy)
            {
                cg.alpha = 1;
                cg.interactable = true;
                cg.blocksRaycasts = true;
            }
        }
    }




}