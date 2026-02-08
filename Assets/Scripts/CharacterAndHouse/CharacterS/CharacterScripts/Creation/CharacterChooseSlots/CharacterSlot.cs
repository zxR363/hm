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
        // 🔥 v25 Check: CharacterArea portalı kendi başına karakter üretemez/yükleyemez
        if (CharacterSelectionManager.Instance != null && 
            CharacterSelectionManager.Instance.characterArea == this)
        {
            CharacterSelectionManager.Instance.SelectSlot(this);
            return;
        }

        string slotName = gameObject.name; 
        string jsonFile = slotName + ".json";
        
        // 🎯 1. JSON Verisi Var mı?
        bool hasJson = PersistenceManager.Exists(jsonFile);
        
        if (hasJson)
        {
            if (characterInstance == null)
            {
                 ClearCharacterArea(); 

                 GameObject basePrefab = Resources.Load<GameObject>("GeneratedCharacters/BaseCharacterPrefab/BaseCharacterPrefab");
                 if (basePrefab != null)
                 {
                      characterInstance = Instantiate(basePrefab, transform);
                      characterInstance.name = slotName + "_Instance";
                      
                      // 🔥 v20: Add marker for deletion logic
                      if (characterInstance.GetComponent<ICharacterPrefab>() == null)
                          characterInstance.AddComponent<ICharacterPrefab>();

                      CharacterSaveData data = PersistenceManager.Load<CharacterSaveData>(jsonFile);
                      CharacterModifier modifier = (CharacterSelectionManager.Instance.characterCreationController != null) 
                          ? CharacterSelectionManager.Instance.characterCreationController.modifier 
                          : FindObjectOfType<CharacterModifier>();

                      if (data != null && modifier != null)
                      {
                          modifier.ApplyVisualState(characterInstance, data);
                          Debug.Log($"[Slot] Reconstruction Successful: {slotName}");
                      }
                 }

                 if (characterInstance != null)
                 {
                     characterInstance.transform.localPosition = slotVisualParent;
                     // 🔥 Size Fix (v14): Always use 0.5f scaling to keep visual consistency
                     characterInstance.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                     if(characterImage != null) characterImage.SetActive(false);
                 }
            }
            CharacterSelectionManager.Instance.SelectSlot(this);
        }
        else
        {
             // --- Boş Slot ---
            if (characterInstance == null)
            {
                GameObject basePrefab = Resources.Load<GameObject>("GeneratedCharacters/BaseCharacterPrefab/BaseCharacterPrefab");
                if (basePrefab != null)
                {
                    characterInstance = Instantiate(basePrefab, transform);
                    characterInstance.transform.localPosition = slotVisualParent;
                    characterInstance.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                    
                    // 🔥 v20: Add marker for deletion logic
                    if (characterInstance.GetComponent<ICharacterPrefab>() == null)
                        characterInstance.AddComponent<ICharacterPrefab>();

                    if(characterImage != null) characterImage.SetActive(false);
                }
            }
            else
            {
                Destroy(characterInstance);
                characterInstance = null;
                if(characterImage != null) characterImage.SetActive(true);
            }
            CharacterSelectionManager.Instance.SelectSlot(this);
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
                
                // 🔥 v20: Add marker for deletion logic
                if (characterInstance.GetComponent<ICharacterPrefab>() == null)
                    characterInstance.AddComponent<ICharacterPrefab>();

                characterInstance.transform.localPosition = slotVisualParent;
                //characterInstance.transform.position += new Vector3(0f, -28f, 0f);
            }
            else
            {
                //Debug.Log("TTTTTT");
                characterInstance = Instantiate(prefab, transform);

                // 🔥 v20: Add marker for deletion logic
                if (characterInstance.GetComponent<ICharacterPrefab>() == null)
                    characterInstance.AddComponent<ICharacterPrefab>();

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