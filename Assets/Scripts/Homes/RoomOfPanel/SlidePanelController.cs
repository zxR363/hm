using UnityEngine;
using DG.Tweening;

public class SlidePanelController : MonoBehaviour
{
    [SerializeField] private RectTransform panel;
    [SerializeField] private float slideDistance = 300f;
    [SerializeField] private float duration = 0.3f;
    [SerializeField] private GameObject itemSelectionPanel;

    public bool IsOpen = false;
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
        if (!IsOpen)
        {
            panel.gameObject.SetActive(true); // Açmadan önce aktif hale getir
            panel.anchoredPosition = closedPos; // Pozisyonu sıfırla
            panel.DOAnchorPos(openedPos, duration).SetEase(Ease.OutCubic);
        }
        else
        {
            panel.DOAnchorPos(closedPos, (duration*0.4f)).SetEase(Ease.InCubic)
                .OnComplete(() => itemSelectionPanel.SetActive(false)); // Animasyon sonrası kapat

            panel.DOAnchorPos(closedPos, duration).SetEase(Ease.InCubic)
                .OnComplete(() => panel.gameObject.SetActive(false)); // Animasyon sonrası kapat
        }

        IsOpen = !IsOpen;
    }

    public void ClosePanel()
    {
        if (!IsOpen)
        {
             return;
        }

        panel.DOAnchorPos(closedPos, duration).SetEase(Ease.InCubic)
            .OnComplete(() => panel.gameObject.SetActive(false));

        IsOpen = false;
    }
}