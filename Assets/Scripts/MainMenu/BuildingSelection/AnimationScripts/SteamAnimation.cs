using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

public class SteamAnimation : MonoBehaviour
{
    [Header("Sprite Ayarları")]
    [SerializeField] private Sprite spriteX1;
    [SerializeField] private Sprite spriteX2;

    [Header("Geçiş Süreleri (saniye)")]
    [SerializeField] private float timeToX2 = 2f;
    [SerializeField] private float timeToX1 = 1f;

    [Header("Pulse Ayarları")]
    [SerializeField] private float pulseScale = 1.2f;
    [SerializeField] private float pulseDuration = 1f;

    private Image image;
    private Tween pulseTween;
    private Coroutine switchRoutine;
    private Vector3 initialScale;

    private void Awake()
    {
        image = GetComponent<Image>();
        initialScale = transform.localScale;
        // TriggerAnimations(); // OnEnable çağıracak
    }

    public void TriggerAnimations()
    {
        if (image == null || spriteX1 == null || spriteX2 == null)
        {
            Debug.LogWarning($"[{name}] SteamAnimation: Eksik bileşen veya referans.");
            return;
        }

        image.sprite = spriteX1;

        if (switchRoutine != null)
            StopCoroutine(switchRoutine);
        switchRoutine = StartCoroutine(SwitchLoop());

        if (pulseTween != null && pulseTween.IsActive())
            pulseTween.Kill();

        // Scale'i resetle
        transform.localScale = initialScale;

        pulseTween = transform
            .DOScale(pulseScale, pulseDuration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutQuad);
    }

    private IEnumerator SwitchLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(timeToX2);
            image.sprite = spriteX2;

            yield return new WaitForSeconds(timeToX1);
            image.sprite = spriteX1;
        }
    }

    private void OnEnable()
    {
        TriggerAnimations();
    }

    private void OnDisable()
    {
        if (pulseTween != null && pulseTween.IsActive())
            pulseTween.Kill();

        if (switchRoutine != null)
            StopCoroutine(switchRoutine);
            
        // Disable olduğunda da scale'i resetlemek iyi olabilir
        transform.localScale = initialScale;
    }
}