using UnityEngine;
using UnityEngine.UI;

public class CharacterSlot : MonoBehaviour
{
    public Button slotButton;
    public Transform slotVisualParent;   // slot prefab’ının konacağı alan

    [Header ("CharacterPrefabRefereans")]
    public GameObject characterInstance;
    
    private GameObject characterImage;
    private GameObject activeCharacter;

    public int slotIndex; // 0–6

    private void Awake()
    {
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
            slotIndex = 6;
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
                characterInstance.transform.localPosition = Vector3.zero;
                //characterInstance.transform.localScale = Vector3.one;
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
        //characterInstance = Instantiate(prefab, transform);
        //characterInstance.transform.localPosition = slotVisualParent.localPosition;
        //characterInstance.transform.position += new Vector3(0f, -28f, 0f);

        //RefreshSlotVisual();

        if (characterImage != null)
            characterImage.SetActive(!prefab); // prefab varsa gizle, yoksa göster

        if (prefab)
        {
            if (activeCharacter != null)
                //Destroy(activeCharacter);

            //Bu alan SlotIndex'ine göre ilgili pozisyon ve boyut ayarlaması yapıyor.
            characterInstance = Instantiate(prefab, transform);
            activeCharacter = characterInstance;
            activeCharacter.transform.localPosition = Vector3.zero;
            activeCharacter.transform.localScale = Vector3.one;
        }
        else
        {
            if (activeCharacter != null)
            {
                //Destroy(activeCharacter);
                //activeCharacter = null;
            }
        }
    }

    //------------Gorsel Image-Prefab switch-----
    public void RefreshSlotVisual()
    {
        bool hasValidPrefab = characterInstance != null && characterInstance.GetComponent<ICharacterPrefab>() != null;

        if (characterImage != null)
            characterImage.SetActive(!hasValidPrefab); // prefab varsa gizle, yoksa göster

        if (hasValidPrefab)
        {
            if (activeCharacter != null)
                Destroy(activeCharacter);

            activeCharacter = Instantiate(characterInstance, transform);
            activeCharacter.transform.localPosition = Vector3.zero;
            activeCharacter.transform.localScale = Vector3.one;
        }
        else
        {
            if (activeCharacter != null)
            {
                Destroy(activeCharacter);
                activeCharacter = null;
            }
        }
    }
    //------------Gorsel Image-Prefab switch-----

    public void ClearSlot()
    {
        if (characterInstance != null)
        {
            Destroy(characterInstance);
            characterInstance = null;
        }
    }
}