using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;


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


    [Header("Slot ve Preview")]
    public CharacterSlot selectedSlot;
    public Transform previewArea;

    private GameObject currentPreviewInstance;

     [Header("CharacterPrefab Kaydetme")]
    //public GameObject previewInstance;
    public int characterCanvasSortOrder = 10; // 🔥 Prefabs sortingLayer değeri 
    public float characterScaleFactor = 0.5f; // 🔥 Prefabs scaleFactor
    public string prefabSavePath = "Assets/Resources/GeneratedCharacters/";

    void Awake()
    {
        Instance = this;
    }

    public void SelectSlot(CharacterSlot slot)
    {

        selectedSlot = slot;


        if (characterCreationManager.previewInstance != null && characterCreationManager.previewInstance.scene.IsValid())
        {
            Destroy(characterCreationManager.previewInstance);
            characterCreationManager.previewInstance = null;
        }

        // 🔄 Slot’taki prefab referansını al
        GameObject prefab = slot.characterInstance;

        // 🔄 Panel geçişi
        characterSlotPanel.SetActive(false);
        characterCreationPanel.SetActive(true);

        // 🔧 Önce previewArea'yı aktif et
        if (!previewArea.gameObject.activeInHierarchy)
        {
            previewArea.gameObject.SetActive(true);
            Debug.Log("PreviewArea aktif hale getirildi");
        }


        if (prefab != null)
        {
            Debug.Log("KONROL");
            //GameObject previewInstance = Instantiate(prefab, previewArea);            
            GameObject previewInstance =  Instantiate(slot.characterInstance);
            previewInstance.name = "CharacterPreview"; // 🔥 ismini sabit tut

            Canvas[] canvases = previewInstance.GetComponentsInChildren<Canvas>(true);
            foreach (Canvas c in canvases)
            {
                Destroy(c);
                Debug.Log("Canvas kaldırıldı: " + c.name);
            }
            
            // Sahneye zorla taşı
            SceneManager.MoveGameObjectToScene(previewInstance, SceneManager.GetActiveScene());

            // 🔄 PreviewArea’ya bağla
            previewInstance.transform.SetParent(previewArea, false); // worldPositionStays = false

            if (previewInstance.transform.parent == previewArea.transform)
            {
                Debug.Log("Gerçekten previewArea altında!");
            }
            else
            {
                Debug.LogWarning("PreviewInstance başka bir parent'a bağlı: " + previewInstance.transform.parent.name);
            }

            Debug.Log("Mask var mı? " + (previewArea.GetComponent<Mask>() != null));
            Debug.Log("RectMask2D var mı? " + (previewArea.GetComponent<RectMask2D>() != null));
            RectTransform rt = previewInstance.GetComponent<RectTransform>();
            rt.anchoredPosition = Vector2.zero;
            Debug.Log("AnchoredPosition: " + rt.anchoredPosition);
            Debug.Log("AnchorMin: " + rt.anchorMin);
            Debug.Log("AnchorMax: " + rt.anchorMax);
            Debug.Log("Pivot: " + rt.pivot);
            Debug.Log("PreviewArea sizeDelta: " + rt.sizeDelta);





            previewInstance.transform.localPosition = Vector3.zero;
            previewInstance.transform.localScale = Vector3.one;

            characterCreationManager.previewInstance = previewInstance;
            characterCreationManager.previewInstance.transform.localScale = Vector3.one;
            
            Debug.Log("----------");
            PrintHierarchy(previewInstance);
            Debug.Log("----------");
            PrintHierarchy(characterCreationManager.previewInstance);
            Debug.Log("----------");

            Debug.Log("HideFlags: " + previewInstance.hideFlags);
            Debug.Log("Sahne adı: " + previewInstance.scene.name);

        }




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

        // // Preview'dan yeni prefab oluştur
        // GameObject updated = Instantiate(characterCreationManager.previewInstance, selectedSlot.slotVisualParent);
        // updated.transform.localPosition = Vector3.zero;
        // updated.transform.localScale = Vector3.one;

        // selectedSlot.characterInstance = updated;


        // 🔥 Karakteri kaydet
        SaveConfirmButtonCharacterPrefab();

        // 🔥 Preview’ı sahneden kaldır
        //Destroy(characterCreationManager.previewInstance);
        if (characterCreationManager.previewInstance != null && characterCreationManager.previewInstance.scene.IsValid())
        {
            Destroy(characterCreationManager.previewInstance);
        }
        else
        {
            Debug.LogWarning("SetCharacter: Asset referansı silinemez");
        }
        characterCreationManager.previewInstance = null;

        // 🔥 İsteğe bağlı: SaveSystem ile karakteri kaydet
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
    }

}