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

                // 2. Check for ScrollRect and ensure its 'content' also has an image
                // This is critical for dragging "between" items if the content container is larger than items
                UnityEngine.UI.ScrollRect scrollRect = contentObj.GetComponent<UnityEngine.UI.ScrollRect>();
                if (scrollRect != null && scrollRect.content != null)
                {
                    EnsureTransparentImage(scrollRect.content.gameObject);
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
            img = obj.AddComponent<UnityEngine.UI.Image>();
            img.color = Color.clear; // Fully transparent
        }
        img.raycastTarget = true;
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
