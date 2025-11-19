using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class DiamondGlow : MonoBehaviour
{
    private Image glowOverlay;
    [SerializeField] private float glowDuration = 0.6f;
    [SerializeField] private float glowAlpha = 0.8f;

    private void Start()
    {
        glowOverlay = transform.GetComponent<Image>();
        if (glowOverlay == null) return;

        glowOverlay.color = new Color(1f, 1f, 1f, 0f); // Başlangıçta görünmez

        glowOverlay.DOFade(glowAlpha, glowDuration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutQuad);
    }
}