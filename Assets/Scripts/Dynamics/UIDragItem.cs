﻿using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;

public class UIDragItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Transform originalParent;
    private Vector3 worldPosBeforeDrag;

    [SerializeField] Vector3 originalScale;
    [SerializeField] Vector2 originalSizeDelta;

    private Canvas canvas;

    [SerializeField] private Transform dragRoot; // HouseCanvas
    [SerializeField] private Collider2D fridgeArea; // Button_Fridge collider
    [SerializeField] private Transform[] spawnPoints; // spawnPoint_1, spawnPoint_2, ...

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = dragRoot.GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        worldPosBeforeDrag = transform.position;
        originalParent = transform.parent;        
        //originalScale = transform.localScale;
        
        transform.SetParent(dragRoot, false); // scale bozulmaz
        transform.position = worldPosBeforeDrag; // pozisyon korunur
        transform.localScale = originalScale; // scale sabitlenir

        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;

    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            dragRoot.GetComponent<RectTransform>(), // HouseCanvas'ın RectTransform'u
            eventData.position,                     // Mouse pozisyonu
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out localPoint
        );

        rectTransform.localPosition = localPoint;
        
        transform.SetParent(dragRoot, false);
        transform.SetAsLastSibling(); // UI hiyerarşisinde en üstte görünür
        // Orijinal width/height'a dön
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (originalSizeDelta.x * 1.1f));
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (originalSizeDelta.y * 1.1f));
        transform.localScale = originalScale; // scale sabitlenir


        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;    
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // Fridge alanı içinde mi?
        bool isInsideFridge = fridgeArea.OverlapPoint(transform.position);

        if (isInsideFridge)
        {
            // En yakın spawnPoint'e yerleştir
            Transform closest = spawnPoints.OrderBy(sp => Vector3.Distance(transform.position, sp.position)).First();
            transform.SetParent(closest, false);
            transform.localScale = originalScale;
            transform.localPosition = Vector3.zero;

            // spawnPoint'in width/height'ını al
            RectTransform spawnRT = closest.GetComponent<RectTransform>();
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, spawnRT.rect.width);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, spawnRT.rect.height);

            Debug.Log("Item Fridge içinde bırakıldı → spawnPoint'e yerleşti.");
        }
        else
        {
            // KitchenPanel'e ait oldu
            transform.SetParent(dragRoot, false); // HouseCanvas altında kalır
            transform.localScale = originalScale;

            // Orijinal width/height'a dön
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, originalSizeDelta.x);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, originalSizeDelta.y);

            Debug.Log("Item Fridge dışına çıktı → KitchenPanel'e ait oldu.");
        }
    }
}