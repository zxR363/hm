using UnityEngine;
using DG.Tweening;

public class SlidePanelController : MonoBehaviour
{
    [SerializeField] private RectTransform panel;
    [SerializeField] private float slideDistance = 5f;
    [SerializeField] private float duration = 0.5f; // Faster
    [SerializeField] private Ease openEase = Ease.OutBack; 
    [SerializeField] private float overshoot = 30f; // Controls the "hardness" and amplitude of the swing
    [SerializeField] private Ease closeEase = Ease.InBack; 
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
            // Use overshoot to increase the swing amplitude
            panel.DOAnchorPos(openedPos, duration).SetEase(openEase, overshoot);
        }
        else
        {
            // Fixed: Single animation to close the panel
            // Use overshoot for closing as well to match the style
            panel.DOAnchorPos(closedPos, duration).SetEase(closeEase, overshoot)
                .OnComplete(() => 
                {
                    if (itemSelectionPanel != null) 
                        itemSelectionPanel.SetActive(false);
                    
                    panel.gameObject.SetActive(false);
                });
        }

        IsOpen = !IsOpen;
    }

    public void ClosePanel()
    {
        if (!IsOpen)
        {
             return;
        }

        panel.DOAnchorPos(closedPos, duration).SetEase(closeEase, overshoot)
            .OnComplete(() => panel.gameObject.SetActive(false));

        IsOpen = false;
    }
}