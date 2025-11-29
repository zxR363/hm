using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSelectionPanelController : MonoBehaviour
{
    public static ItemSelectionPanelController Instance;

    [SerializeField] private GameObject panelRoot;
    [SerializeField] private List<TabButton> tabButtons;
    [SerializeField] private List<GameObject> tabContents;

    private bool isActive = false;

    [SerializeField] private int panelSortingOrder = 100; // Increased to ensure visibility
    [SerializeField] private int contentSortingOrder = 100; // Default 100 as requested
    public int ContentSortingOrder => contentSortingOrder;

    private void Awake()
    {
        Instance = this;
        panelRoot.SetActive(false);
    }

    public void TogglePanel()
    {
        if (isActive == false)
        {
            isActive = true;
            OpenPanel();
        }
        else
        {
            isActive = false;
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
        canvas.sortingOrder = contentSortingOrder; // Updated to use contentSortingOrder for the main panel too

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

        // Apply sorting order to ALL canvases under panelRoot (Recursive)
        ApplySortingOrderToAll();

        SelectTab(0); // Varsayılan ilk tab
    }

    private void ApplySortingOrderToAll()
    {
        if (panelRoot == null) return;

        // Find all Canvas components including inactive ones under the panelRoot
        Canvas[] allCanvases = panelRoot.GetComponentsInChildren<Canvas>(true);

        foreach (Canvas c in allCanvases)
        {
            // Check if this Canvas belongs to an ItemSelection object
            if (c.GetComponent<ItemSelection>() != null)
            {
                c.overrideSorting = true;
                c.sortingOrder = contentSortingOrder + 1;
            }
            else
            {
                // For all other structural parts (Panel, Viewport, Content, etc.)
                c.overrideSorting = true;
                c.sortingOrder = contentSortingOrder;
            }
        }
    }

    private void EnsureTransparentImage(GameObject obj)
    {
        UnityEngine.UI.Image img = obj.GetComponent<UnityEngine.UI.Image>();
        if (img == null)
        {
            // Only add if no other graphic is present? 
            // Actually, for catching raycasts, we need a Graphic. Image is best.
            img = obj.AddComponent<UnityEngine.UI.Image>();
            // Unity bazen tamamen şeffaf (alpha=0) objeleri raycast dışı bırakabilir.
            // Bu yüzden çok çok az görünür (neredeyse şeffaf) yapıyoruz.
            img.color = new Color(0, 0, 0, 0.004f);
        }
        img.raycastTarget = true;

        // Ensure it has a Canvas and Raycaster to catch events properly
        if (obj.GetComponent<Canvas>() == null)
        {
            Canvas c = obj.AddComponent<Canvas>();
            c.overrideSorting = true;
            // Inspector'dan seçilen değeri kullanıyoruz
            c.sortingOrder = contentSortingOrder;
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
        Debug.Log("SELECT TAB TIKLANIYOR INDEX=" + index);
        for (int i = 0; i < tabContents.Count; i++)
        {
            tabContents[i].SetActive(i == index);
        }
    }

}
