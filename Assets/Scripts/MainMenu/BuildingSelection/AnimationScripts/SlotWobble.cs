using UnityEngine;
using DG.Tweening;

public class SlotWobble : MonoBehaviour
{
    [Header("Pendulum Settings")]
    [SerializeField] private float swayAmount = 0.5f; // Bend miktarı
    [SerializeField] private float duration = 1f;
    
    private RectTransform rectTransform;
    private UIBendEffect bendEffect;
    private Tween swayTween;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        
        // "Image" ismindeki alt objeyi bul
        Transform imageChild = transform.Find("Image");
        
        // Eğer "Image" yoksa, ilk çocuğu al (Yedek plan)
        if (imageChild == null && transform.childCount > 0)
        {
            imageChild = transform.GetChild(0);
        }

        if (imageChild != null)
        {
            // UIBendEffect'i bu alt objeye ekle/al
            bendEffect = imageChild.GetComponent<UIBendEffect>();
            if (bendEffect == null)
            {
                bendEffect = imageChild.gameObject.AddComponent<UIBendEffect>();
                bendEffect.exponent = 2.5f; 
            }
        }
        else
        {
            // Hiç çocuk yoksa kendine ekle (Fallback)
            bendEffect = GetComponent<UIBendEffect>();
            if (bendEffect == null)
            {
                bendEffect = gameObject.AddComponent<UIBendEffect>();
                bendEffect.exponent = 2.5f;
            }
        }
    }

    public void TriggerWobble()
    {
        if (swayTween != null && swayTween.IsActive())
            swayTween.Kill();

        if (bendEffect != null)
        {
            // Reset
            bendEffect.SetBend(0f);

            // Sarkaç hareketi (Bend animasyonu)
            // 0 -> +Amount -> 0 -> -Amount -> 0
            Sequence sequence = DOTween.Sequence();
            
            // Bend miktarını tween'le
            sequence.Append(DOTween.To(() => bendEffect.bendAmount, x => bendEffect.SetBend(x), swayAmount, duration * 0.25f).SetEase(Ease.OutSine))
                    .Append(DOTween.To(() => bendEffect.bendAmount, x => bendEffect.SetBend(x), 0f, duration * 0.25f).SetEase(Ease.InSine))
                    .Append(DOTween.To(() => bendEffect.bendAmount, x => bendEffect.SetBend(x), -swayAmount, duration * 0.25f).SetEase(Ease.OutSine))
                    .Append(DOTween.To(() => bendEffect.bendAmount, x => bendEffect.SetBend(x), 0f, duration * 0.25f).SetEase(Ease.InSine));

            swayTween = sequence;
        }
    }

    private void OnDisable()
    {
        if (swayTween != null)
            swayTween.Kill();
            
        if (bendEffect != null)
            bendEffect.SetBend(0f);
    }
}