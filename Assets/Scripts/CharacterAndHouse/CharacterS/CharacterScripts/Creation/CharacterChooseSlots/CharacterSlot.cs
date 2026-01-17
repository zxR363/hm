using UnityEngine;
using UnityEngine.UI;

public class CharacterSlot : MonoBehaviour
{
    public Button slotButton;
    private Vector3 slotVisualParent;   // slot prefab’ının konacağı alan

    [Header ("CharacterPrefabRefereans")]
    public GameObject characterInstance;

    private GameObject characterImage;
    private GameObject activeCharacter;

    public int slotIndex; // 0–6
    private int constSlotAreaIndex = 18; //Slot INDEX AREA

    [Header("CharacterDeleteButton")]
    public Image fillCircle; // Inspector'da FillCircle atanmalı

    private void Awake()
    {
        //TODO: Koordinat kontrol edilecek
        slotVisualParent = new Vector3(0f, 150f, 0f);
        // "ShowArea" altındaki "characterImage" objesini bul
        Transform showArea = transform.Find("ShowArea");
        if (showArea != null)
        {
            Transform imageTransform = showArea.Find("characterImage");
            if (imageTransform != null)
            {
                characterImage = imageTransform.gameObject;
            }
            else
            {
                Debug.LogWarning($"CharacterSlot_{slotIndex}: 'characterImage' ShowArea içinde bulunamadı.");
            }
        }
        else
        {
            Debug.LogWarning($"CharacterSlot_{slotIndex}: 'ShowArea' objesi bulunamadı.");
        }

        // Örn: "CharacterSlot_3" → slotIndex = 3
        string name = gameObject.name;
        int underscoreIndex = name.LastIndexOf('_');

        if(name == "CharacterArea")
        {
            slotIndex = constSlotAreaIndex;
            return;
        }

        if (underscoreIndex >= 0 && underscoreIndex < name.Length - 1)
        {
            string indexStr = name.Substring(underscoreIndex + 1);
            if (int.TryParse(indexStr, out int parsedIndex))
            {
                slotIndex = parsedIndex;
            }
            else
            {
                Debug.LogWarning($"CharacterSlot: İsmin sonundaki index çözülemedi → {name}");
            }
        }
        else
        {
            Debug.LogWarning($"CharacterSlot: Geçersiz isim formatı → {name}");
        }
    }

    private void Start()
    {
        if (slotButton != null)
            slotButton.onClick.AddListener(OnClick);
    }

    public void OnClick()
    {
        // 🎯 Slot ismine göre kayıtlı prefab var mı kontrol et
        string slotName = gameObject.name; // örn: "CharacterSlot_3"
        string prefabPath = $"GeneratedCharacters/{slotName}";
        GameObject savedPrefab = Resources.Load<GameObject>(prefabPath);

        if (savedPrefab != null)
        {
            // --- DURUM A: Kayıtlı Karakter Var ---
            if (characterInstance == null)
            {
                 characterInstance = Instantiate(savedPrefab, transform);
                 characterInstance.transform.localPosition = slotVisualParent;
                 characterInstance.name = savedPrefab.name;
                 
                 if(characterImage != null) characterImage.SetActive(false);
            }
            else
            {
                // Zaten var, ama yanlışlıkla başka bir şey varsa yenile
                if (!characterInstance.name.Contains(slotName))
                {
                    Destroy(characterInstance);
                    characterInstance = Instantiate(savedPrefab, transform);
                    characterInstance.transform.localPosition = slotVisualParent;
                    characterInstance.name = savedPrefab.name;
                    if(characterImage != null) characterImage.SetActive(false);
                }
            }
            CharacterSelectionManager.Instance.SelectSlot(this);
        }
        else
        {
            // --- DURUM B: Boş Slot (Toggle Mantığı) ---
            if (characterInstance == null)
            {
                // 1. Tık: Base Karakteri Göster
                GameObject basePrefab = Resources.Load<GameObject>("GeneratedCharacters/BaseCharacterPrefab/BaseCharacterPrefab");
                if (basePrefab != null)
                {
                    characterInstance = Instantiate(basePrefab, transform);
                    characterInstance.transform.localPosition = slotVisualParent; // (0, 150, 0)
                    characterInstance.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                    
                    if(characterImage != null) characterImage.SetActive(false);
                    
                    CharacterSelectionManager.Instance.SelectSlot(this);
                }
            }
            else
            {
                // 2. Tık: İptal Et (Base Karakteri Sil, Image Göster)
                Destroy(characterInstance);
                characterInstance = null;
                
                if(characterImage != null) characterImage.SetActive(true);

                CharacterSelectionManager.Instance.SelectSlot(this); // null gidecek ve preview temizlenecek
            }
        }
    }

    //Karakter previewArea'da belirlenip Confirm yapıldıktan sonra burada 
    //Kaydedilmiş prefab objesi ilgili slot alanına yerleştiriliyor.
    public void SetCharacter(GameObject prefab)
    {
        if (characterInstance != null && characterInstance.scene.IsValid())
        {
            Destroy(characterInstance);
        }
        else
        {
            Debug.LogWarning("SetCharacter: Asset referansı silinemez");
        }
        if (characterImage != null && characterImage.scene.IsValid())
            characterImage.SetActive(!prefab); // prefab varsa gizle, yoksa göster

        if (prefab)
        {
            if(slotIndex == constSlotAreaIndex)
            {
                ClearCharacterArea();
                Debug.Log("CCCCCC");
                characterInstance = Instantiate(prefab, transform);
                characterInstance.transform.localPosition = slotVisualParent;
                //characterInstance.transform.position += new Vector3(0f, -28f, 0f);
            }
            else
            {
                //Debug.Log("TTTTTT");
                characterInstance = Instantiate(prefab, transform);
                characterInstance.transform.localPosition = slotVisualParent;
                //characterInstance.transform.position += new Vector3(0f, -28f, 0f);
            }
        }
        else
        {
            if (characterInstance != null)
            {
                Destroy(characterInstance);
                characterInstance = null;
            }
        }
    }

    public void ClearCharacterArea()
    {
        foreach (Transform child in transform)
        {
            if (child.GetComponent<ICharacterPrefab>() != null)
            {
                Destroy(child.gameObject);
            }
        }
    }

    public void ClearSlot()
    {
        if (characterInstance != null)
        {
            Destroy(characterInstance);
            characterInstance = null;
        }
    }
}