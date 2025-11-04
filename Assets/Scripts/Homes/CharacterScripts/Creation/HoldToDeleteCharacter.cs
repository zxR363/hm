using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class HoldToDeleteCharacter : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("CharacterArea")]
    [SerializeField] private Transform characterAreaRoot;

    private Image fillCircle;
    private GameObject targetPrefab;
    private GameObject characterImage;
    private Transform slotRoot;

    private float holdTime;
    private bool isHolding;

    public float holdDuration = 1.5f;

    private void Start()
    {
        // FillCircle'ı kendi altından bul
        fillCircle = transform.Find("FillCircle")?.GetComponent<Image>();

        // Slot kökünü al (DeleteButton → CharacterSlot_)
        slotRoot = transform.parent;

        // characterImage → ShowArea altında
        characterImage = slotRoot.Find("ShowArea/characterImage")?.gameObject;
    }

    private void Update()
    {
        // 🎯 ICharacterPrefab interface’ine sahip aktif objeyi bul
        targetPrefab = FindCharacterPrefabUnder(slotRoot);

        // 🎯 Buton görünürlüğünü dinamik ayarla
        if (targetPrefab != null && targetPrefab.activeSelf)
        {
            if (!gameObject.activeSelf) gameObject.SetActive(true);
        }
        else if (characterImage != null && characterImage.activeSelf)
        {
            if (gameObject.activeSelf) gameObject.SetActive(false);
        }

        // 🎯 Silme animasyonu
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
            //var prefabComponent = targetPrefab.GetComponent<ICharacterPrefab>();
            string rawName = targetPrefab.name;
            string prefabName = NormalizePrefabName(rawName);

            Destroy(targetPrefab);

            if (!string.IsNullOrEmpty(prefabName))
            {
                DeletePrefabAsset(prefabName);
            }

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

            // 🎯 CharacterArea'daki prefab ile eşleşiyorsa → sahneden kaldır
            if (characterAreaRoot != null)
            {
                foreach (Transform child in characterAreaRoot)
                {
                    var areaComponent = child.GetComponent<ICharacterPrefab>();
                    if (areaComponent != null)
                    {
                        string areaName = NormalizePrefabName(areaComponent.name);
                        if (areaName == prefabName)
                        {
                            Destroy(child.gameObject);
                            break;
                        }
                    }
                }
            }
        }
    }


    // 🎯 Interface tabanlı prefab arayıcı
    private GameObject FindCharacterPrefabUnder(Transform parent)
    {
        foreach (Transform child in parent)
        {
            var prefabComponent = child.GetComponent<ICharacterPrefab>();
            if (prefabComponent != null && child.gameObject.activeSelf)
            {
                return child.gameObject;
            }

            // Recursive arama
            GameObject found = FindCharacterPrefabUnder(child);
            if (found != null)
                return found;
        }

        return null;
    }

    private void DeletePrefabAsset(string prefabName)
    {
        Debug.Log("SİLİNEN PREFAB NAME=" + prefabName);
    #if UNITY_EDITOR
        string path = $"Assets/Resources/GeneratedCharacters/{prefabName}.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
        {
            bool success = AssetDatabase.DeleteAsset(path);
            if (success)
                Debug.Log($"🧹 Prefab asset silindi: {path}");
            else
                Debug.LogWarning($"❌ Prefab asset silinemedi: {path}");
        }
    #endif
    }

    private string NormalizePrefabName(string rawName)
    {
        if (string.IsNullOrEmpty(rawName))
            return "";

        return rawName.Replace("(Clone)", "").Trim();
    }



}
