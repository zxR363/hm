using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSelectionPanelController : MonoBehaviour
{
    public static ItemSelectionPanelController Instance;

    [SerializeField] private GameObject panelRoot;
    [SerializeField] private List<TabButton> tabButtons;
    [SerializeField] private List<GameObject> tabContents;

    [Header("Visibility Limits")]
    [SerializeField] private Transform topLimit;
    [SerializeField] private Transform bottomLimit;
    public Transform TopLimit => topLimit;
    public Transform BottomLimit => bottomLimit;

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

        // Apply sorting order to Panel Root
        if (canvas != null)
        {
            canvas.overrideSorting = true;
            canvas.sortingOrder = contentSortingOrder;
        }

        // Fix for scroll dead zones
        foreach (var contentObj in tabContents)
        {
            if (contentObj != null)
            {
                EnsureTransparentImage(contentObj);

                UnityEngine.UI.ScrollRect scrollRect = contentObj.GetComponent<UnityEngine.UI.ScrollRect>();
                if (scrollRect != null)
                {
                    if (scrollRect.content != null)
                    {
                        EnsureTransparentImage(scrollRect.content.gameObject);
                    }

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
            // Ensure every Canvas has a GraphicRaycaster so it can receive events
            if (c.GetComponent<UnityEngine.UI.GraphicRaycaster>() == null)
            {
                c.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }

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
