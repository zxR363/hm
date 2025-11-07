using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class HoldToDeleteBuilding : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("FillCircle")]
    [SerializeField] private Image fillCircle;

    private GameObject buildingImage;
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

        // 🎯 Slot kökünü bul (BuildingButton_ objesi)
        slotRoot = FindSlotRoot(deleteContainer);

        // 🎯 Image → BuildingBUtton_XXX altında
        buildingImage = slotRoot.Find("Image")?.gameObject;
    }

    private void Update()
    {

        // 🎯 Basılı tutma animasyonu
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
        var slotSelector = slotRoot.GetComponent<BuildingSlotSelector>();
        if (slotSelector != null)
        {
            slotSelector.resetBuildingSlot(); // veya resetBuildingSlot() gibi bir fonksiyon
        }
        else
        {
            Debug.LogWarning("❌ BuildingSlotSelector bulunamadı!");
        }

    }

    private GameObject FindCharacterPrefabUnder(Transform parent)
    {
        foreach (Transform child in parent)
        {
            var prefabComponent = child.GetComponent<ICharacterPrefab>();
            if (prefabComponent != null && child.gameObject.activeSelf)
                return child.gameObject;

            GameObject found = FindCharacterPrefabUnder(child);
            if (found != null)
                return found;
        }

        return null;
    }

    private Transform FindSlotRoot(Transform current)
    {
        while (current != null)
        {
            if (current.name.StartsWith("BuildingButton_"))
                return current;

            current = current.parent;
        }

        return null;
    }

}
