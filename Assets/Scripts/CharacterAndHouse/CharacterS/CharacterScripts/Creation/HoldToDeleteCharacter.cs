using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class HoldToDeleteCharacter : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("CharacterArea")]
    [SerializeField] private Transform characterAreaRoot;

    [Header("FillCircle")]
    [SerializeField] private Image fillCircle;

    private GameObject targetPrefab;
    private GameObject characterImage;
    private Transform slotRoot;

    private float holdTime;
    private bool isHolding;

    public float holdDuration = 1.5f;

    private void Start()
    {
        // 🎯 FillCircle artık DeleteButton ile aynı seviyede, Delete altında
        Transform deleteContainer = transform.parent;
        fillCircle = deleteContainer.Find("FillCircle")?.GetComponent<Image>();
        if (fillCircle != null)
        {
            fillCircle.transform.SetSiblingIndex(0); // Görsel olarak arkada kalması için
            fillCircle.raycastTarget = false; // Tıklamayı engellemesin
        }

        // 🎯 Slot kökünü bul (CharacterSlot_ objesi)
        slotRoot = FindSlotRoot(deleteContainer);

        // 🎯 characterImage → ShowArea altında
        characterImage = slotRoot.Find("ShowArea/characterImage")?.gameObject;
    }

    private float _checkTimer = 0f;
    private const float CHECK_INTERVAL = 0.2f; // Check 5 times per second instead of 60+

    private void Update()
    {
        // 🎯 OTIMIZASYON: Her frame aramak yerine belirli aralıklarla ara
        _checkTimer += Time.deltaTime;
        
        if (_checkTimer >= CHECK_INTERVAL)
        {
            _checkTimer = 0f;
            // 🎯 Aktif prefab'ı bul
            targetPrefab = FindCharacterPrefabUnder(slotRoot);

            // 🎯 Buton görünürlüğünü ayarla
            if (targetPrefab != null && targetPrefab.activeSelf)
            {
                if (!gameObject.activeSelf) gameObject.SetActive(true);
            }
            else if (characterImage != null && characterImage.activeSelf)
            {
                if (gameObject.activeSelf) gameObject.SetActive(false);
            }
        }

        // 🎯 Basılı tutma animasyonu (Burası her frame çalışmalı ki akıcı olsun)
        if (isHolding)
        {
            holdTime += Time.deltaTime;
            if (fillCircle != null)
                fillCircle.fillAmount = holdTime / holdDuration;

            if (holdTime >= holdDuration)
            {
                DeleteTarget();
                ResetHold();
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isHolding = true;
        holdTime = 0f;
        if (fillCircle != null)
            fillCircle.fillAmount = 0f;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ResetHold();
    }

    private void ResetHold()
    {
        isHolding = false;
        holdTime = 0f;
        if (fillCircle != null)
            fillCircle.fillAmount = 0f;
    }

    private void DeleteTarget()
    {
        if (targetPrefab != null && targetPrefab.activeSelf)
        {
            // 🎯 v20: Get slot name from hierarchy
            string slotName = slotRoot.name; // e.g., "CharacterSlot_3"
            string jsonFile = slotName + ".json";

            Debug.Log($"[Delete] Attempting to delete character data: {jsonFile}");

            // 1. Delete the JSON file
            PersistenceManager.Delete(jsonFile);

            // 2. Destroy the character instance
            Destroy(targetPrefab);

            // 3. Reset Slot Visual via the Component
            CharacterSlot slot = slotRoot.GetComponent<CharacterSlot>();
            if (slot != null)
            {
                slot.characterInstance = null; // Clear reference
            }

            // 4. Show the fallback image
            if (characterImage != null)
            {
                characterImage.SetActive(true);

                CanvasGroup cg = characterImage.GetComponent<CanvasGroup>();
                if (cg != null)
                {
                    cg.alpha = 1f;
                    cg.interactable = true;
                    cg.blocksRaycasts = true;
                }
            }

            // 5. Sync with CharacterArea (v27 - Refined)
            CharacterSlot slotComponent = slotRoot.GetComponent<CharacterSlot>();
            if (CharacterSelectionManager.Instance != null && slotComponent != null)
            {
                // 🔥 Notify with index for targeted sync
                CharacterSelectionManager.Instance.NotifySlotDeleted(slotComponent.slotIndex);
                
                // Fallback for name-based sync (v21)
                if (CharacterSelectionManager.Instance.selectedSlot != null && 
                    CharacterSelectionManager.Instance.selectedSlot.gameObject.name == slotName)
                {
                    CharacterSelectionManager.Instance.ClearCharacterArea();
                }
            }

            // 6. If there's an active character in the CreationArea (fallback cleanup)
            if (characterAreaRoot != null)
            {
                foreach (Transform child in characterAreaRoot)
                {
                    var areaComponent = child.GetComponent<ICharacterPrefab>();
                    if (areaComponent != null)
                    {
                        // Simply clear the area if was showing this slot (optional security)
                        Destroy(child.gameObject);
                        break;
                    }
                }
            }
            
            Debug.Log($"[Delete] Character successfully deleted from {slotName}");
        }
    }

    private GameObject FindCharacterPrefabUnder(Transform parent)
    {
        if (parent == null) return null;
        
        foreach (Transform child in parent)
        {
            var prefabComponent = child.GetComponent<ICharacterPrefab>();
            if (prefabComponent != null && child.gameObject.activeSelf)
                return child.gameObject;

            // Optional: check children if nested deeper
            if (child.childCount > 0)
            {
                GameObject found = FindCharacterPrefabUnder(child);
                if (found != null) return found;
            }
        }

        return null;
    }

    private Transform FindSlotRoot(Transform current)
    {
        while (current != null)
        {
            if (current.name.StartsWith("CharacterSlot_"))
                return current;

            current = current.parent;
        }

        return null;
    }

    private string NormalizePrefabName(string rawName)
    {
        if (string.IsNullOrEmpty(rawName))
            return "";

        return rawName.Replace("(Clone)", "").Trim();
    }
}
