using UnityEngine;
using UnityEngine.UI;

public class CharacterSlot : MonoBehaviour
{
    public Button slotButton;
    public Transform slotVisualParent;   // slot prefab’ının konacağı alan

    [Header ("CharacterPrefabRefereans")]
    public GameObject characterInstance;


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
        characterInstance = Instantiate(prefab, transform);
        characterInstance.transform.localPosition = slotVisualParent.localPosition;
        characterInstance.transform.position += new Vector3(0f, -28f, 0f);
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