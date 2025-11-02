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
    private int constSlotAreaIndex = 6;

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
        if (characterInstance == null)
        {
            // 🔥 Kullanıcı boş slot'a tıkladı → BaseCharacterPrefab ile oluştur
            GameObject basePrefab = Resources.Load<GameObject>("GeneratedCharacters/BaseCharacterPrefab/BaseCharacterPrefab");
            if (basePrefab != null)
            {
                characterInstance = Instantiate(basePrefab, transform);
                characterInstance.transform.localPosition = slotVisualParent;
                //characterInstance.transform.localScale = Vector3.one;
            }
        }
        else
        {
            // 🎯 Slot ismine göre prefab yükle
            string slotName = gameObject.name; // örn: "CharacterSlot_3"
            string prefabPath = $"GeneratedCharacters/{slotName}/{slotName}";

            GameObject slotPrefab = Resources.Load<GameObject>(prefabPath);
            if (slotPrefab != null)
            {
                characterInstance = Instantiate(slotPrefab, transform);
                characterInstance.transform.localPosition = slotVisualParent;
            }
            else
            {
                Debug.LogWarning($"❌ Prefab bulunamadı: {prefabPath}");
            }
        }
        CharacterSelectionManager.Instance.SelectSlot(this);
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
                Debug.Log("TTTTTT");
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