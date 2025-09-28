using UnityEngine;
using DG.Tweening;

public class SlidePanelController : MonoBehaviour
{
    [SerializeField] private RectTransform panel;
    [SerializeField] private float slideDistance = 300f;
    [SerializeField] private float duration = 0.3f;

    private bool isOpen = false;
    private Vector2 closedPos;
    private Vector2 openedPos;

    private void Awake()
    {
        closedPos = panel.anchoredPosition;
        openedPos = closedPos + new Vector2(-slideDistance, 0f);
        panel.gameObject.SetActive(false); // Başlangıçta kapalı
    }

    public void TogglePanel()
    {
        if (!isOpen)
        {
            panel.gameObject.SetActive(true); // Açmadan önce aktif hale getir
            panel.anchoredPosition = closedPos; // Pozisyonu sıfırla
            panel.DOAnchorPos(openedPos, duration).SetEase(Ease.OutCubic);
        }
        else
        {
            panel.DOAnchorPos(closedPos, duration).SetEase(Ease.InCubic)
                .OnComplete(() => panel.gameObject.SetActive(false)); // Animasyon sonrası kapat
        }

        isOpen = !isOpen;
    }
}