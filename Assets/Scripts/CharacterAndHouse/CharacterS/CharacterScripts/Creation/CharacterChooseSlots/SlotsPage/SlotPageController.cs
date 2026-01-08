using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class SlotPageController : MonoBehaviour
{
    [Header("Slot Sayfaları")]
    public GameObject[] slotPages; // örn: SlotsPart_0, SlotsPart_1, SlotsPart_2

    [Header("Geçiş Butonları")]
    public Button leftButton;
    public Button rightButton;

    [Header("Sayfa Numarası")]
    public TextMeshProUGUI pageNumberText; // CharacterPageNumbers/TextPage içindeki Text

    private int currentPageIndex = 0;
    private bool isTransitioning = false;

    private void Start()
    {
        for (int i = 0; i < slotPages.Length; i++)
        {
            CanvasGroup cg = slotPages[i].GetComponent<CanvasGroup>();
            if (cg == null) 
            {
               // cg = slotPages[i].AddComponent<CanvasGroup>();
               // Debug.LogWarning($"[SlotPageController] Missing CanvasGroup on {slotPages[i].name}");
            }
            cg.alpha = (i == currentPageIndex) ? 1f : 0f;
            slotPages[i].SetActive(i == currentPageIndex);
        }

        UpdateButtonStates();
        UpdatePageNumber();

        //UpdatePageVisibility();

        leftButton.onClick.AddListener(GoLeft);
        rightButton.onClick.AddListener(GoRight);
    }

    private void GoLeft()
    {
        if (currentPageIndex > 0 && !isTransitioning)
        {
            StartCoroutine(TransitionToPage(currentPageIndex - 1, Vector2.left));
        }
    }

    private void GoRight()
    {
        if (currentPageIndex < slotPages.Length - 1 && !isTransitioning)
        {
            StartCoroutine(TransitionToPage(currentPageIndex + 1, Vector2.right));
        }
    }

    private IEnumerator TransitionToPage(int newIndex, Vector2 direction)
    {
        isTransitioning = true;

        // 🔥 Buton scale efekti
        Button clickedButton = (direction == Vector2.left) ? leftButton : rightButton;
        Transform buttonVisual = clickedButton.transform;
        Vector3 originalScale = buttonVisual.localScale;
        buttonVisual.localScale = originalScale * 0.7f;

        GameObject oldPage = slotPages[currentPageIndex];
        GameObject newPage = slotPages[newIndex];

        RectTransform oldRT = oldPage.GetComponent<RectTransform>();
        RectTransform newRT = newPage.GetComponent<RectTransform>();

        CanvasGroup oldCG = oldPage.GetComponent<CanvasGroup>();
        CanvasGroup newCG = newPage.GetComponent<CanvasGroup>();

        newPage.SetActive(true);
        newRT.anchoredPosition = direction * Screen.width;

        // 🔥 Alt objelerin CanvasGroup'larını aktif hale getir
        UpdateAllChildCanvasGroups(newPage.transform, true);


        float duration = 0.33f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;

            oldCG.alpha = Mathf.Lerp(1f, 0f, t);
            newCG.alpha = Mathf.Lerp(0f, 1f, t);

            oldRT.anchoredPosition = Vector2.Lerp(Vector2.zero, -direction * Screen.width, t);
            newRT.anchoredPosition = Vector2.Lerp(direction * Screen.width, Vector2.zero, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        oldCG.alpha = 0f;
        newCG.alpha = 1f;
        oldRT.anchoredPosition = Vector2.zero;
        newRT.anchoredPosition = Vector2.zero;

        oldPage.SetActive(false);
        currentPageIndex = newIndex;

        // 🔄 Buton scale geri dönüş
        buttonVisual.localScale = originalScale;

        UpdateButtonStates();
        UpdatePageNumber();

        isTransitioning = false;
    }

    private void UpdateButtonStates()
    {
        leftButton.interactable = currentPageIndex > 0;
        rightButton.interactable = currentPageIndex < slotPages.Length - 1;
    }

    private void UpdatePageNumber()
    {
        if (pageNumberText != null && slotPages.Length > currentPageIndex)
        {
            string pageName = slotPages[currentPageIndex].name; // örn: "SlotsPart_2"
            string[] parts = pageName.Split('_');

            if (parts.Length == 2 && int.TryParse(parts[1], out int pageNum))
            {
                pageNumberText.text = "#" + pageNum.ToString();
            }
            else
            {
                pageNumberText.text = "-";
                Debug.LogWarning($"Sayfa adı çözümlenemedi: {pageName}");
            }
        }
    }


    private void UpdateAllChildCanvasGroups(Transform parent, bool isActive)
    {
        foreach (Transform child in parent)
        {
            CanvasGroup cg = child.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = isActive ? 1f : 0f;
                cg.interactable = isActive;
                cg.blocksRaycasts = isActive;
            }

            // 🔁 Alt çocukları da kontrol et (recursive)
            if (child.childCount > 0)
            {
                UpdateAllChildCanvasGroups(child, isActive);
            }
        }
    }


}