using UnityEngine;

public class FoodItemController : MonoBehaviour
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

        // canvasGroup null değilse gizle
        if (canvasGroup != null)
        {
            HideCompletely();
        }
    }

    void Start()
    {
        transform.localPosition = Vector3.zero;
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

        Debug.Log("FOOD ITEM OPENED CALISIYOR");
    }

    public void HideCompletely()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        transform.localScale = originalScale * 0.8f;
        gameObject.SetActive(false); // Sahneden tamamen kaldır

        Debug.Log("FOOD ITEM HIDE CALISIYOR");
    }
}
