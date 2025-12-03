using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabButton : MonoBehaviour
{
    [SerializeField] private int tabIndex;

    private ItemSelectionPanelController controller;
    private CanvasGroup canvasGroup;

    public void Initialize(ItemSelectionPanelController ctrl)
    {
        Debug.Log("ITEMSELECTION INJECT EDILDI");
        this.controller = ctrl;
    }
    
    public void OnClick()
    {
        if (controller != null)
        {
            controller.SelectTab(tabIndex);
        }
    }

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    private void Start()
    {
        // Optimization: Disable manual visibility check.
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
        }
    }

    // Removed CheckVisibilityRoutine and IsVisibleInViewport for performance
}
