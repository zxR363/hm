using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class GarbageBinController : MonoBehaviour
{
    public static GarbageBinController Instance;

    private CanvasGroup canvasGroup;
    private RectTransform binRect;
    private Vector3 initialScale;

    private void Awake()
    {
        Instance = this;
        
        // Auto-discover components as requested
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            Debug.LogError("[GarbageBinController] CanvasGroup not found on the GameObject!");
        }

        // Search for Image in children (including self) to define the hit area
        // User said: "Image found under itself"
        Image binImage = GetComponentInChildren<Image>();
        if (binImage != null)
        {
            binRect = binImage.rectTransform;
        }
        else
        {
            // Fallback to self rect if no image found (e.g. invisible area)
            binRect = GetComponent<RectTransform>();
        }
        
        if (binRect != null) initialScale = binRect.localScale;

        Hide(); // Initially hidden
    }

    public void Show()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }
    }

    public void Hide()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
        // Reset scale just in case
        if (binRect != null)
        {
            binRect.localScale = initialScale;
            binRect.localRotation = Quaternion.identity; // Reset rotation
            binRect.DOKill(); // Kill any running tweens
        }
    }

    public void OnHoverEnter()
    {
        if (binRect != null)
        {
            binRect.DOKill();
            binRect.DOScale(initialScale * 1.2f, 0.2f).SetEase(Ease.OutBack);
            // Rotate -15 degrees (Clockwise/Right)
            binRect.DORotate(new Vector3(0, 0, -15f), 0.2f).SetEase(Ease.OutBack);
        }
    }

    public void OnHoverExit()
    {
        if (binRect != null)
        {
            binRect.DOKill();
            binRect.DOScale(initialScale, 0.2f).SetEase(Ease.OutBack);
            // Reset rotation
            binRect.DORotate(Vector3.zero, 0.2f).SetEase(Ease.OutBack);
        }
    }

    public bool IsPointerOverBin(Vector2 screenPosition)
    {
        if (binRect == null) return false;
        
        // Note: Using null camera for standard Screen Space Overlay or unassigned World Space
        // Adjust if you are using specific Camera rendering
        return RectTransformUtility.RectangleContainsScreenPoint(binRect, screenPosition, null);
    }
}
