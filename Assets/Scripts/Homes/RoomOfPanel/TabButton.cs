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
        // Optimization: Disable manual visibility check.
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
        }
    }

    // Removed CheckVisibilityRoutine and IsVisibleInViewport for performance
}
