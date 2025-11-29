using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSelectionPanelController : MonoBehaviour
{
    public static ItemSelectionPanelController Instance;

    [SerializeField] private GameObject panelRoot;
    [SerializeField] private List<TabButton> tabButtons;
    [SerializeField] private List<GameObject> tabContents;

    private bool isActive=false;

    [SerializeField] private int panelSortingOrder = 100; // Increased to ensure visibility

    private void Awake()
    {
        Instance = this;
        panelRoot.SetActive(false);
    }

    public void TogglePanel()
    {
        if(isActive==false)
        {
            isActive=true;
            OpenPanel();
        }
        else
        {
            isActive=false;
            ClosePanel();
        }
    }

    public void OpenPanel()
    {
        panelRoot.SetActive(true);
        
        // Ensure the panel renders on top of other Canvas elements
        Canvas canvas = panelRoot.GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = panelRoot.AddComponent<Canvas>();
            panelRoot.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }
        
        canvas.overrideSorting = true;
        canvas.sortingOrder = panelSortingOrder;

        // Fix for scroll dead zones: Ensure all tab contents have a raycast target (transparent image)
        foreach (var contentObj in tabContents)
        {
            if (contentObj != null)
            {
                // 1. Ensure the root tab object (likely holding ScrollRect) has an image
                EnsureTransparentImage(contentObj);

                // 2. Check for ScrollRect and ensure its 'content' has Image
                UnityEngine.UI.ScrollRect scrollRect = contentObj.GetComponent<UnityEngine.UI.ScrollRect>();
                if (scrollRect != null)
                {
                    if (scrollRect.content != null)
                    {
                        EnsureTransparentImage(scrollRect.content.gameObject);
                    }
                    
                    // Viewport'a da ekleyelim garanti olsun
                    if (scrollRect.viewport != null)
                    {
                        EnsureTransparentImage(scrollRect.viewport.gameObject);
                    }
                }
            }
        }
        
        SelectTab(0); // Varsayılan ilk tab
    }

    private void EnsureTransparentImage(GameObject obj)
    {
        UnityEngine.UI.Image img = obj.GetComponent<UnityEngine.UI.Image>();
        if (img == null)
        {
            // Only add if no other graphic is present? 
            // Actually, for catching raycasts, we need a Graphic. Image is best.
            img = obj.AddComponent<UnityEngine.UI.Image>();
            img.color = new Color(0, 0, 0, 0.004f); 
        }
        img.raycastTarget = true;

        // Ensure it has a Canvas and Raycaster to catch events properly
        if (obj.GetComponent<Canvas>() == null)
        {
            Canvas c = obj.AddComponent<Canvas>();
            c.overrideSorting = true;
            // Content'i itemların arkasına atmak için sorting order'ı düşürüyoruz.
            // Items muhtemelen 100 veya hiyerarşi sırasına göre çiziliyor.
            // Biz Content'i 90 yaparsak, Items (100) önde kalır.
            c.sortingOrder = panelSortingOrder - 10; 
        }
        if (obj.GetComponent<UnityEngine.UI.GraphicRaycaster>() == null)
        {
            obj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }
    }

    public void ClosePanel()
    {
        panelRoot.SetActive(false);
    }

    public void SelectTab(int index)
    {
        Debug.Log("SELECT TAB TIKLANIYOR INDEX="+index);
        for (int i = 0; i < tabContents.Count; i++)
        {
            tabContents[i].SetActive(i == index);
        }
    }
    
}
