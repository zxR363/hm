using UnityEngine;

public class CharacterSelectionManager : MonoBehaviour
{
    public static CharacterSelectionManager Instance;

    [Header("Slot ve Preview")]
    public CharacterSlot selectedSlot;
    public Transform previewArea;

    private GameObject currentPreviewInstance;

    void Awake()
    {
        Instance = this;
    }

    public void SelectSlot(CharacterSlot slot)
    {
        selectedSlot = slot;

        // 🔄 Önceki preview varsa sil
        if (currentPreviewInstance != null)
        {
            Destroy(currentPreviewInstance);
            currentPreviewInstance = null;
        }

        // 🔄 Slot’taki prefab referansını al
        GameObject prefab = slot.characterInstance;
        if (prefab != null)
        {
            GameObject previewInstance = Instantiate(prefab, previewArea);
            previewInstance.name = "CharacterPreview"; // 🔥 ismini sabit tut
            //previewInstance.transform.localPosition = Vector3.zero;
            previewInstance.transform.localScale = Vector3.one;

            currentPreviewInstance = previewInstance;
        }
    }

    public void ConfirmCharacter()
    {
        if (selectedSlot == null || currentPreviewInstance == null)
        {
            Debug.LogWarning("ConfirmCharacter: Slot veya preview eksik");
            return;
        }

        // 🔄 Slot prefab’ını güncelle
        selectedSlot.SetCharacter(currentPreviewInstance);

        // 🔥 İsteğe bağlı: preview’ı sahneden kaldır
        Destroy(currentPreviewInstance);
        currentPreviewInstance = null;

        // 🔥 İsteğe bağlı: SaveSystem ile karakteri kaydet
    }
}