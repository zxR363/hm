using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabButton : MonoBehaviour
{
    [SerializeField] private int tabIndex;

    private CanvasGroup canvasGroup;

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
        StartCoroutine(CheckVisibilityRoutine());
    }

    public void OnClick()
    {
        ItemSelectionPanelController.Instance.SelectTab(tabIndex);
    }

    private System.Collections.IEnumerator CheckVisibilityRoutine()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();

            if (ItemSelectionPanelController.Instance != null)
            {
                bool isVisible = IsVisibleInViewport();
                canvasGroup.alpha = isVisible ? 1f : 0f;
                canvasGroup.blocksRaycasts = isVisible;
            }
        }
    }

    private bool IsVisibleInViewport()
    {
        if (ItemSelectionPanelController.Instance == null) return true;

        Transform leftLimit = ItemSelectionPanelController.Instance.LeftLimit;
        Transform rightLimit = ItemSelectionPanelController.Instance.RightLimit;

        if (leftLimit == null || rightLimit == null) return true;

        float itemX = transform.position.x;

        // Check horizontal position relative to limits
        // Assuming Left X < Right X
        bool isAfterLeft = itemX > leftLimit.position.x;
        bool isBeforeRight = itemX < rightLimit.position.x;

        return isAfterLeft && isBeforeRight;
    }
}
