using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabContentLoader : MonoBehaviour
{
    [SerializeField] private Transform contentParent;
    [SerializeField] private GameObject itemButtonPrefab;
    [SerializeField] private string categoryFolder; // Örn: "Furniture", "Food"

    public void LoadItems()
    {
        // foreach (Transform child in contentParent)
        //     Destroy(child.gameObject);

        // Sprite[] previews = Resources.LoadAll<Sprite>("ItemIcons/" + categoryFolder);
        // foreach (Sprite preview in previews)
        // {
        //     GameObject btn = Instantiate(itemButtonPrefab, contentParent);
        //     ItemButton ib = btn.GetComponent<ItemButton>();
        //     ib.Setup(preview.name, preview, "Açıklama: " + preview.name);
        // }
    }
}
