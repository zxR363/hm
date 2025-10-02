using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSelection : MonoBehaviour
{
    //[SerializeField] private string targetSortingLayer = "UI"; // Değiştirmek istediğin layer adı
    [SerializeField] private int targetSortingOrder = 0;       // İsteğe bağlı: sıralama önceliği

    private void Awake()
    {
        transform.localScale = new Vector3(0.5f,0.5f,0.5f);
        Canvas canvas = GetComponent<Canvas>();
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

}
