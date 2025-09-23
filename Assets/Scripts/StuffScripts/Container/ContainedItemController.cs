using UnityEngine;

public class ContainedItemController : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    private Vector3 originalScale;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }           

        originalScale = transform.localScale;
        HideCompletely();
    }

    public void ShowFully()
    {       
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                Debug.LogError($"CanvasGroup missing on {gameObject.name}");
                return;
            }
        }

        gameObject.SetActive(true);
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        transform.localScale = originalScale;
    }

    public void HideCompletely()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        transform.localScale = originalScale * 0.8f;
        gameObject.SetActive(false);
    }
}
