using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSelection : MonoBehaviour
{
    //[SerializeField] private string targetSortingLayer = "UI"; // Değiştirmek istediğin layer adı
    [SerializeField] private int targetSortingOrder = 0;       // İsteğe bağlı: sıralama önceliği
    
    private RectTransform itemRect;
    private Canvas itemCanvas;
    private RectTransform viewPortRect;
    private Canvas canvas;

    private void Awake()
    {
        transform.localScale = new Vector3(0.5f,0.5f,0.5f);
        canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.overrideSorting = true; // Sorting Layer'ı değiştirebilmek için gerekli
            //canvas.sortingLayerName = targetSortingLayer;
            canvas.sortingOrder = targetSortingOrder;

            //Debug.Log($"Canvas sorting layer set to {targetSortingLayer} with order {targetSortingOrder}");
            Debug.Log($"Canvas sorting layer set to with order {targetSortingOrder}");
        }
        else
        {
            Debug.LogWarning("Canvas component not found on this GameObject.");
        }
    }

    private void Start()
    {
        itemRect = GetComponent<RectTransform>();
        itemCanvas = GetComponent<Canvas>();

        // 2 üst parent Viewport ise:
        Transform viewportTransform = transform.parent?.parent;
        if (viewportTransform != null)
        {
            viewPortRect = viewportTransform.GetComponent<RectTransform>();
        }
        else
        {
            Debug.LogWarning("Viewport not found for item: " + gameObject.name);
        }

    }

    private void Update()
    {
        Transform grandParent = transform.parent?.parent;
        if (grandParent == null || grandParent != viewPortRect.transform)
            return;

        if (IsVisibleInViewport())
        {
            //itemCanvas.sortingOrder = 5; // görünür
            //itemCanvas.enabled = true;
            CanvasGroup cg = GetComponent<CanvasGroup>();
            cg.alpha = 1f;
            cg.blocksRaycasts = true;

        }
        else
        {
            //itemCanvas.sortingOrder = -10; // görünmez (arka plana atılır)
            //itemCanvas.enabled = false;
            CanvasGroup cg = GetComponent<CanvasGroup>();
            cg.alpha = 0f;
            cg.blocksRaycasts = false;
        }
    }

    private bool IsVisibleInViewport()
    {

        Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

        Vector3[] itemCorners = new Vector3[4];
        itemRect.GetWorldCorners(itemCorners);

        foreach (var corner in itemCorners)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(viewPortRect, corner, cam))
                return true;
        }

        return false;

    }

}
