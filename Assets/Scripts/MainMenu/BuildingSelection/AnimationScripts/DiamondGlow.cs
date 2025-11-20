using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class DiamondGlow : MonoBehaviour
{
    private Image glowOverlay;

    [SerializeField] private float glowDuration = 0.6f;
    [SerializeField] private float glowAlpha = 0.8f;

    private Tween glowTween;

    private void Awake()
    {
        glowOverlay = GetComponent<Image>();
        TriggerAnimations();
    }

    public void TriggerAnimations()
    {
        if (glowOverlay == null)
        {
            Debug.LogWarning($"[{name}] DiamondGlow: Image bileşeni eksik.");
            return;
        }

        glowOverlay.color = new Color(1f, 1f, 1f, 0f); // Başlangıçta görünmez

        if (glowTween != null && glowTween.IsActive())
            glowTween.Kill();

        glowTween = glowOverlay
            .DOFade(glowAlpha, glowDuration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutQuad);
    }

    private void OnDisable()
    {
        if (glowTween != null && glowTween.IsActive())
            glowTween.Kill();
    }
}