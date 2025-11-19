using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening; // DoTween namespace

public class SteamAnimation : MonoBehaviour
{
    [Header("Sprite Ayarları")]
    [SerializeField] private Sprite spriteX1;
    [SerializeField] private Sprite spriteX2;

    [Header("Geçiş Süreleri (saniye)")]
    [SerializeField] private float timeToX2 = 1f;
    [SerializeField] private float timeToX1 = 1f;

    [Header("Pulse Ayarları")]
    [SerializeField] private float pulseScale = 1.06f;
    [SerializeField] private float pulseDuration = 1.2f;

    private Image image;
    private Tween pulseTween;

    private void Start()
    {
        image = GetComponent<Image>();
        if (image == null || spriteX1 == null || spriteX2 == null)
        {
            Debug.LogWarning("Eksik bileşen veya referans.");
            return;
        }

        image.sprite = spriteX1;
        //StartCoroutine(SwitchLoop());
        StartPulse();
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

    private void StartPulse()
    {
        pulseTween = transform
            .DOScale(pulseScale, pulseDuration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutQuad);
    }

    private void OnDisable()
    {
        if (pulseTween != null && pulseTween.IsActive())
            pulseTween.Kill();
    }


}