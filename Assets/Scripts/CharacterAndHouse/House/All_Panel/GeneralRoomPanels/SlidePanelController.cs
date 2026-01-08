using UnityEngine;
using DG.Tweening;

public class SlidePanelController : MonoBehaviour
{
    [SerializeField] private RectTransform panel;
    [SerializeField] private float slideDistance = 5f; // User defined
    [SerializeField] private float duration = 0.5f; // User defined (Faster)
    [SerializeField] private Ease openEase = Ease.OutBack; 
    [SerializeField] private float overshoot = 30f; // User defined
    [SerializeField] private Ease closeEase = Ease.InBack; 
    [SerializeField] private GameObject itemSelectionPanel;

    public bool IsOpen = false;
    private Vector2 closedPos;
    private Vector2 openedPos;

    private void Awake()
    {
        InitializePanel();
    }

    private void OnEnable()
    {
        InitializePanel();
        FixLayoutConflict();
    }

    private void InitializePanel()
    {
        if(panel == null) panel = GetComponent<RectTransform>(); 
        
        if (Application.isPlaying && panel != null)
        {
             closedPos = panel.anchoredPosition;
             openedPos = closedPos + new Vector2(-slideDistance, 0f);
        }
    }

    private void FixLayoutConflict()
    {
        // FIX GRAPHIC REBUILD LOOP (Only checks once on Enable now)
        var group = GetComponent<UnityEngine.UI.HorizontalLayoutGroup>();
        
        if (group != null)
        {
            if (group.childForceExpandWidth) 
            {
                group.childForceExpandWidth = false; 
            }
            if (group.childForceExpandHeight) 
            {
                group.childForceExpandHeight = false;
            }
        }
    }

    public void TogglePanel()
    {
        if (!IsOpen)
        {
            Debug.Log($"[SlidePanelController] TogglePanel: Opening. (IsOpen was false)");
            panel.gameObject.SetActive(true);
            panel.anchoredPosition = closedPos;
            panel.DOAnchorPos(openedPos, duration).SetEase(openEase, overshoot);
        }
        else
        {
            Debug.Log($"[SlidePanelController] TogglePanel: Closing. (IsOpen was true).");
            SlidePanelItemButton.ResetAll();

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
        if (!Application.isPlaying) return;
        if (!IsOpen) return;

        SlidePanelItemButton.ResetAll();

        panel.DOAnchorPos(closedPos, duration).SetEase(closeEase, overshoot)
            .OnComplete(() => 
            {
                panel.gameObject.SetActive(false);
            });

        IsOpen = false;
    }
}