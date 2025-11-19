using UnityEngine;
using DG.Tweening;

public class CloudArrowAnimator : MonoBehaviour
{
    private CanvasGroup canvasGroup;

    private void Start()
    {
        Debug.Log("ARROW TETIKLENDI");
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // 🎯 Başlangıç alpha
        canvasGroup.alpha = 0f;

        // 🔁 Yanıp sönme: 0 → 1 → 0 → ...
        canvasGroup.DOFade(1f, 0.5f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutQuad);

        // ⏳ 3 saniye sonra gizle
        Invoke(nameof(HideSelf), 3f);
    }

    private void HideSelf()
    {
        gameObject.SetActive(false);
    }
}