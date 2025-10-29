using UnityEngine;
using UnityEngine.UI;

public class CharacterSlot : MonoBehaviour
{
    public int slotIndex;
    public Button slotButton;

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
            GameObject basePrefab = Resources.Load<GameObject>("BaseCharacterPrefab");
            if (basePrefab != null)
            {
                characterInstance = Instantiate(basePrefab, transform);
                characterInstance.transform.localPosition = Vector3.zero;
            }
        }

        CharacterSelectionManager.Instance.SelectSlot(this);
    }

    public void SetCharacter(GameObject newCharacter)
    {
        if (characterInstance != null)
            Destroy(characterInstance);

        characterInstance = Instantiate(newCharacter, transform);
        characterInstance.transform.localPosition = Vector3.zero;
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