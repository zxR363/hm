using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using static UnityEngine.GraphicsBuffer;


#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class CharacterSelectionManager : MonoBehaviour
{
    public static CharacterSelectionManager Instance;

    [Header("Managers")]
    //public CharacterCreationManager characterCreationManager; // ❌ Deleted
    public CharacterCreationController characterCreationController; // ✅ New Controller

    [Header("Character Panels")]
    public GameObject characterSlotPanel;
    public GameObject characterCreationPanel;

    [Header("OptionGridPanel-Content")]
    public GameObject optionGridContent;


    [Header("Character Slot ve Area")]
    public CharacterSlot characterArea; //Seçilen karakterin gösterileceği yer
    public CharacterSlot selectedSlot;
    public CharacterSlot[] allSlots; // 0–5 CharacterArea Yok
    public Transform previewArea; //
    private int activeSlotIndex = -1; //Seçilmiş olunan slot indexi
    private int characterAreaIndex;

    private GameObject currentPreviewInstance;

     [Header("CharacterPrefab Kaydetme")]
    public int characterCanvasSortOrder = 10; // 🔥 Prefabs sortingLayer değeri 
    public float characterScaleFactor = 0.5f; // 🔥 Prefabs scaleFactor
    public string prefabSavePath = "Assets/Resources/GeneratedCharacters/";

    private Vector3 slotVisualParent;

    private GameObject activeCharacter;
    private List<CanvasGroup> fadeTargets;

    void Awake()
    {
        //TODO: Koordinat kontrol edilecek
        slotVisualParent = new Vector3(0f, 150f, 0f);
        characterAreaIndex = allSlots.Length;
        Debug.Log("AreaIndex="+characterAreaIndex);
        Instance = this;
    }

    private void Start()
    {
        foreach (CharacterSlot slot in allSlots)
        {
            if (slot.slotIndex < characterAreaIndex)
            {
                // 🎯 v13: Sadece JSON var mı diye bak ve OnClick ile yükle
                string jsonFile = slot.gameObject.name + ".json";
                if (PersistenceManager.Exists(jsonFile))
                {
                    slot.OnClick(); 
                }
            }
        }
    }

    public void SelectSlot(CharacterSlot slot)
    {

        if (slot.slotIndex < characterAreaIndex)
        {
            // Slot 1–6 → Preview’a göster
            activeSlotIndex = slot.slotIndex;
            ShowInCharacterArea(slot.characterInstance);
        }

        else if(slot.slotIndex == characterAreaIndex)
        {
            //-------CharacterPreviewArea boş iken edit yapılamaz------
            // 1. Safety Check (v25): Prevent crash on allSlots[activeSlotIndex]
            if (activeSlotIndex == -1)
            {
                Debug.LogWarning("[SelectionManager] Blocked: No active index.");
                return;
            }

            bool controlFlag=false;
            foreach (Transform child in characterArea.transform)
            {
                if (child.GetComponent<ICharacterPrefab>() != null)
                {
                    controlFlag = true;
                    break; // 🔥 Optimization: stop at first find
                }
            }

            if(controlFlag == false)
            {
                Debug.LogWarning("[SelectionManager] Blocked Access: CharacterArea is empty.");
                return;
            }
            //-------CharacterPreviewArea boş iken edit yapılamaz------

            // 🔄 Panel geçişi
            characterSlotPanel.SetActive(false);
            characterCreationPanel.SetActive(true);

            //RectTransform rt = characterCreationPanel.GetComponent<RectTransform>();
            //StartCoroutine(AnimatePanelIn(rt)); // sağdan kayarak gelsin

            RectTransform panelRT = characterCreationPanel.GetComponent<RectTransform>();
            CanvasGroup cg = characterCreationPanel.GetComponent<CanvasGroup>();
            if (cg == null)
            {
                // READ-ONLY
                // Debug.LogWarning($"[CharacterSelectionManager] Missing CanvasGroup on Panel. Fix Prefab.");
            }

            StartCoroutine(SlideDiagonalAndFadeIn(panelRT, cg));

            selectedSlot = allSlots[activeSlotIndex];
            
            // 🔥 Duplicate Fix: Edit moduna geçerken sahnedeki karakteri temizle
            ClearCharacterArea();

            ResetOptionGridToDefault();

            // 🔥 v17: Pass the actual slot so we can use its ID and fresh reconstruction
            StartCoroutine(DelayedPreview(selectedSlot));
        }        
    }

    //--------------------CharacterAREA---------------------
    public void ShowInCharacterArea(GameObject prefab)
    {
        // PreviewArea’ya gösterim
        ClearCharacterArea();

        // GameObject finalPrefab = prefab;

        // // Eğer prefab null ise → slot ismine göre Resources'tan yüklemeyi dene
        // if (finalPrefab == null && selectedSlot != null)
        // {
        //     string slotName = selectedSlot.gameObject.name; // örn: "CharacterSlot_3"
        //     string prefabPath = $"GeneratedCharacters/{slotName}";
        //     finalPrefab = Resources.Load<GameObject>(prefabPath);

        //     if (finalPrefab == null)
        //     {
        //         GameObject preview = Instantiate(prefab, characterArea.transform);
        //         preview.transform.localPosition = slotVisualParent;
        //         Vector3 updateScale = new Vector3(0.5f, 0.5f, 0.5f);
        //         preview.transform.localScale = updateScale;
        //         return;
        //     }
        //     else
        //     {
        //         // PreviewArea’ya gösterim
        //         GameObject preview = Instantiate(finalPrefab, characterArea.transform);
        //         preview.transform.localPosition = slotVisualParent;
        //         Vector3 updateScale = new Vector3(0.5f, 0.5f, 0.5f);
        //         preview.transform.localScale = updateScale;
        //     }
        // }

                if (prefab != null)
                {
                    GameObject preview = Instantiate(prefab, characterArea.transform);
                    
                    // 🔥 v21: Add marker for deletion sync
                    if (preview.GetComponent<ICharacterPrefab>() == null)
                        preview.AddComponent<ICharacterPrefab>();

                    // 🔥 Layout Fix: Reset RectTransform
                    RectTransform rt = preview.GetComponent<RectTransform>();
                    if (rt != null)
                    {
                        rt.anchorMin = new Vector2(0.5f, 0.5f);
                        rt.anchorMax = new Vector2(0.5f, 0.5f);
                        rt.pivot = new Vector2(0.5f, 0.5f);
                        rt.anchoredPosition = Vector2.zero; 
                    }

                    // 🔥 Alpha Fix: Ensure CanvasGroup is visible
                    CanvasGroup cg = preview.GetComponent<CanvasGroup>();
                    if (cg != null) cg.alpha = 1f;

                    preview.transform.localPosition = slotVisualParent;
                    Vector3 updateScale = new Vector3(0.5f, 0.5f, 0.5f);
                    preview.transform.localScale = updateScale;
                    preview.SetActive(true); 
                }
    }

    public void ClearCharacterArea()
    {
        foreach (Transform child in characterArea.transform)
        {
            if (child.GetComponent<ICharacterPrefab>() != null)
            {
                Destroy(child.gameObject);
            }
        }
    }

    //--------------------CharacterAREA---------------------

    //----------------------CHARACTER PANEL AYARLAMA ISLEMLERI-------

    // !!!! DİKKAT: Dinamik olarak CharacterCreationPanel'deki PreviewArea'nın altına
    // ilgili prefab'ı eklemeye imkan tanıyor.
    // SetActive gibi bir durumdan kaynaklı olarak Hierarchy de gözükmüyor
    // o sebeple Coroutine ile yapıyoruz bu işlemi 1 sonraki frame de koyuyor.
    private IEnumerator DelayedPreview(CharacterSlot slot)
    {
        // Panel aktif olana kadar bekle
        float timeout = 0.5f;
        while (!characterCreationPanel.activeInHierarchy && timeout > 0)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        if (!previewArea.gameObject.activeInHierarchy)
        {
            Debug.LogWarning("PreviewArea hala aktif değil!");
            yield break;
        }

        // 🔥 Perfect Cleanup (v16): Clear old previews immediately
        for (int i = previewArea.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(previewArea.GetChild(i).gameObject);
        }

        // 🔥 v17 Reconstruction: Always start from base prefab
        GameObject previewInstance = Instantiate(characterCreationController.characterPrefab);
        previewInstance.name = "CharacterPreview";

        // UI bağlama
        RectTransform rt = previewInstance.GetComponent<RectTransform>();
        rt.SetParent(previewArea, false);

        // 🔧 Pozisyon ve layout ayarları
        rt.localScale = Vector3.one; 
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = Vector2.zero; 
        rt.offsetMax = Vector2.zero;

        // 🔥 v17: Load JSON and apply to fresh base
        if (slot != null && characterCreationController != null)
        {
            string jsonFile = slot.gameObject.name + ".json";
            CharacterSaveData data = PersistenceManager.Load<CharacterSaveData>(jsonFile);
            if (data != null)
            {
                characterCreationController.modifier.ApplyVisualState(previewInstance, data);
            }
            characterCreationController.SetCurrentCharacter(previewInstance);
        }
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

        // // 3. Default kategoriye set et (örneğin “Skin”)
        // // 3. Default kategoriye set et (örneğin “Skin”)
        // if(characterCreationController != null) ...
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
        if (selectedSlot == null || characterCreationController == null || characterCreationController.currentCharacter == null)
        {
            Debug.LogWarning("ConfirmCharacter: Slot veya karakter eksik");
            return;
        }

        // 🔥 1. JSON Olarak Kaydet (Unified Save)
        string slotId = selectedSlot.gameObject.name;
        CharacterSaveData data = characterCreationController.modifier.CaptureVisualState(characterCreationController.currentCharacter, slotId);
        PersistenceManager.Save(slotId + ".json", data);
        Debug.Log($"[Manager] Character recipe saved to JSON: {slotId}");

        // 🔥 2. Slotu JSON'dan Yeniden Yükle
        selectedSlot.ClearSlot();
        selectedSlot.OnClick(); 

        // 🔥 3. Panel Geçişi
        ConfirmButtonPanelSwitch();
    }

    public void BackButtonCharacter()
    {
        if (selectedSlot == null || characterCreationController == null)
        {
            Debug.LogWarning("BackButtonCharacter: Slot veya Controller eksik");
            // Yine de paneli kapatmayı dene
            characterCreationPanel.SetActive(false);
            characterSlotPanel.SetActive(true);
            return;
        }

        // 🔥 Preview’ı sahneden kaldır
        if (characterCreationController.currentCharacter != null)
        {
            Destroy(characterCreationController.currentCharacter);
            characterCreationController.SetCurrentCharacter(null);
        }

        // 🔄 Duplicate Fix: Sahneye karakteri geri yükle
        if (selectedSlot != null)
        {
             ShowInCharacterArea(selectedSlot.characterInstance);
        }

        // 🔄 Panel geçişi
        characterCreationPanel.SetActive(false);
        characterSlotPanel.SetActive(true);
    }

    // v13: SaveConfirmButtonCharacterPrefab DELETED as part of Pure JSON Architecture.

    //Confirm mesajı sonrasında Paneller arası geçiş yapmamızı sağlayan fonksiyon
    //Confirm'butonunda fonksiyon olarak 
    public void ConfirmButtonPanelSwitch()
    {
        // 🔄 Panel geçişi
        characterCreationPanel.SetActive(false);
        characterSlotPanel.SetActive(true);

        // 🎯 Unique Names to avoid Variable Shadowing (v25)
        Transform slotsContainer = characterSlotPanel.transform.Find("AllSlots");

        if (slotsContainer == null)
        {
            Debug.LogWarning("AllSlots container not found!");
            return;
        }

        // SlotPanel altındaki tüm CanvasGroup bileşenlerini topla
        CanvasGroup[] allGroups = characterSlotPanel.GetComponentsInChildren<CanvasGroup>(true);
        List<CanvasGroup> panelFadeTargets = new List<CanvasGroup>(allGroups);

        // Tüm GameObject’leri tarayıp eksik olanlara CanvasGroup ekle
        Transform[] panelChildren = characterSlotPanel.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in panelChildren)
        {
            if (child.name == "DeleteButton")
            {
                //StartCoroutine(ActivateAndReveal(child.gameObject));

                child.gameObject.SetActive(true);
                CanvasGroup cg = child.gameObject.GetComponent<CanvasGroup>();
                if (cg != null)
                {
                    cg.alpha = 0f;
                    cg.interactable = false;
                    cg.blocksRaycasts = false;
                    //fadeTargets.Add(cg);
                }
            }
            else
            {
                if (child.GetComponent<CanvasGroup>() == null)
                {
                    // CanvasGroup cg = child.gameObject.AddComponent<CanvasGroup>();
                    // Debug.LogWarning($"[CharacterSelectionManager] Missing CanvasGroup on {child.name}");
                }
            }            
        }

        StartCoroutine(FadeInAllAtOnce(panelFadeTargets, 0.8f));

    }

    private IEnumerator ActivateAndReveal(GameObject target)
    {
        target.SetActive(true);
        yield return null; // 🔄 1 frame bekle

        CanvasGroup cg = target.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = 1f;
            cg.interactable = true;
            cg.blocksRaycasts = true;
        }
    }




    //----------------------CHARACTER PANEL AYARLAMA ISLEMLERI-------

    //--------------ANIMATION----------------------------

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

    //--------------ANIMATION----------------------------


}