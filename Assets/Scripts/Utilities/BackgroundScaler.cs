using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class BackgroundScaler : MonoBehaviour
{
    private Image image;
    private AspectRatioFitter aspectRatioFitter;

    private void Start()
    {
        image = GetComponent<Image>();
        SetupAspectRatioFitter();
    }

    private void OnEnable()
    {
        // Sahne aktif olduğunda veya obje açıldığında tekrar kontrol et
        SetupAspectRatioFitter();
    }

    public void SetupAspectRatioFitter()
    {
        if (image == null)
            image = GetComponent<Image>();

        if (image.sprite == null)
            return;

        // AspectRatioFitter bileşenini al veya ekle
        aspectRatioFitter = GetComponent<AspectRatioFitter>();
        if (aspectRatioFitter == null)
        {
            aspectRatioFitter = gameObject.AddComponent<AspectRatioFitter>();
        }

        // Ayarları yapılandır
        aspectRatioFitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        
        // Resmin orijinal en-boy oranını kullan
        float spriteRatio = image.sprite.rect.width / image.sprite.rect.height;
        aspectRatioFitter.aspectRatio = spriteRatio;

        // RectTransform ayarlarını sıfırla (Stretch moduna geçmesi için)
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = Vector2.zero;
    }
}
