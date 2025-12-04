using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSelectionPanelController : MonoBehaviour
{
    public static ItemSelectionPanelController Instance;

    [SerializeField] private GameObject panelRoot;
    [SerializeField] private GameObject tabGroup; // New reference
    [SerializeField] private List<TabButton> tabButtons;
    [SerializeField] private List<GameObject> tabContents;

    [Header("Visibility Limits")]
    [SerializeField] private Transform topLimit;
    [SerializeField] private Transform bottomLimit;
    [SerializeField] private Transform leftLimit; // New
    [SerializeField] private Transform rightLimit; // New
    public Transform TopLimit => topLimit;
    public Transform BottomLimit => bottomLimit;
    public Transform LeftLimit => leftLimit; // New
    public Transform RightLimit => rightLimit; // New

    private bool isActive = false;

    [SerializeField] private int panelSortingOrder = 100; // Increased to ensure visibility
    [SerializeField] private int contentSortingOrder = 101; // Default 100 as requested
    public int ContentSortingOrder => contentSortingOrder;

    private List<Vector3> tabButtonInitialScales;

    private void Awake()
    {
        Instance = this;
        panelRoot.SetActive(false);

        // Capture initial scales of tab buttons and inject dependency
        tabButtonInitialScales = new List<Vector3>();
        foreach (var btn in tabButtons)
        {
            if (btn != null)
            {
                tabButtonInitialScales.Add(btn.transform.localScale);
                btn.Initialize(this); // Inject reference
            }
            else
            {
                tabButtonInitialScales.Add(Vector3.one); // Fallback
            }
        }
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
        isActive = true; // Ensure this is true!
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

        // Fix for scroll dead zones (Tab Contents)
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

        // Fix for scroll dead zones (TabGroup)
        if (tabGroup != null)
        {
            UnityEngine.UI.ScrollRect tabScroll = tabGroup.GetComponent<UnityEngine.UI.ScrollRect>();
            if (tabScroll != null)
            {
                EnsureTransparentImage(tabGroup); // Ensure root has image
                if (tabScroll.content != null) EnsureTransparentImage(tabScroll.content.gameObject);
                if (tabScroll.viewport != null) EnsureTransparentImage(tabScroll.viewport.gameObject);
            }
        }

        // Apply sorting order to ALL canvases under panelRoot (Recursive)
        ApplySortingOrderToAll();

        SelectTab(0); // Varsayılan ilk tab

        // Force UI update to ensure new images/masks are rendered correctly
        StartCoroutine(RefreshUIRoutine());
    }

    public void ClosePanel()
    {
        StopCoroutine("CheckVisibilityRoutine");
        panelRoot.SetActive(false);
    }

    //SECILEN TabGroup'un calistigi fonksiyon 
    public void SelectTab(int index)
    {
        Debug.Log("SELECT TAB TIKLANIYOR INDEX=" + index);

        // 1. Ensure all tabs are active (to prevent state loss)
        for (int i = 0; i < tabContents.Count; i++)
        {
            if (tabContents[i] != null && !tabContents[i].activeSelf)
            {
                tabContents[i].SetActive(true);
            }
        }

        // 2. Reset everything to default visibility (Items -> 102, etc.)
        ApplySortingOrderToAll();

        // 3. Hide inactive tabs by lowering their sorting order to -50
        for (int i = 0; i < tabContents.Count; i++)
        {
            if (i == index) continue; // Skip active tab

            if (tabContents[i] != null)
            {
                Canvas[] childCanvases = tabContents[i].GetComponentsInChildren<Canvas>(true);
                foreach (var c in childCanvases)
                {
                    c.sortingOrder = -50;
                }
            }
        }

        // 4. Scale the selected tab button
        for (int i = 0; i < tabButtons.Count; i++)
        {
            if (tabButtons[i] != null && i < tabButtonInitialScales.Count)
            {
                if (i == index)
                {
                    tabButtons[i].transform.localScale = tabButtonInitialScales[i] * 1.15f;
                }
                else
                {
                    tabButtons[i].transform.localScale = tabButtonInitialScales[i];
                }
            }
        }

        // 5. Force UI update
        StartCoroutine(RefreshUIRoutine());
    }

    private List<Canvas> _cachedItemCanvases = new List<Canvas>();
    private List<Canvas> _cachedTabCanvases = new List<Canvas>();

    private IEnumerator RefreshUIRoutine()
    {
        // Wait for end of frame to ensure all SetActive/Layout calculations are done
        yield return new WaitForEndOfFrame();
        
        // Force rebuild
        Canvas.ForceUpdateCanvases();
        
        // Extra safety: toggle layout groups if any (generic fix)
        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(panelRoot.GetComponent<RectTransform>());
        
        // --- CACHE LISTS FOR PERFORMANCE ---
        _cachedItemCanvases.Clear();
        _cachedTabCanvases.Clear();

        if (panelRoot != null)
        {
            // Cache ItemSelection Canvases
            var items = panelRoot.GetComponentsInChildren<ItemSelection>(true);
            foreach (var item in items)
            {
                Canvas c = item.GetComponent<Canvas>();
                if (c != null) _cachedItemCanvases.Add(c);
            }

            // Cache TabButton Canvases
            foreach (var btn in tabButtons)
            {
                if (btn != null)
                {
                    Canvas c = btn.GetComponent<Canvas>();
                    if (c != null) _cachedTabCanvases.Add(c);
                }
            }
        }
        // -----------------------------------

        // Start visibility check routine
        StopCoroutine("CheckVisibilityRoutine");
        StartCoroutine("CheckVisibilityRoutine");
    }

    private IEnumerator CheckVisibilityRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(0.1f); // Optimized interval
        
        while (isActive)
        {
            if (panelRoot == null) yield break;

            // 1. Check ItemSelections (Vertical)
            foreach (Canvas c in _cachedItemCanvases)
            {
                if (c == null) continue;
                
                bool isVisible = true;
                Vector3 pos = c.transform.position;

                if (topLimit != null && pos.y > topLimit.position.y) isVisible = false;
                if (bottomLimit != null && pos.y < bottomLimit.position.y) isVisible = false;

                if (isVisible)
                {
                    if (c.sortingOrder == -50) c.sortingOrder = contentSortingOrder + 3;
                }
                else
                {
                    if (c.sortingOrder != -50) c.sortingOrder = -50;
                }
            }

            // 2. Check TabButtons (Horizontal)
            foreach (Canvas c in _cachedTabCanvases)
            {
                if (c == null) continue;

                bool isVisible = true;
                Vector3 pos = c.transform.position;

                if (leftLimit != null && pos.x < leftLimit.position.x) isVisible = false;
                if (rightLimit != null && pos.x > rightLimit.position.x) isVisible = false;

                if (isVisible)
                {
                    if (c.sortingOrder == -50) c.sortingOrder = contentSortingOrder + 3;
                }
                else
                {
                    if (c.sortingOrder != -50) c.sortingOrder = -50;
                }
            }

            yield return wait;
        }
    }

    private void ApplySortingOrderToAll()
    {
        if (panelRoot == null) return;

        // Ensure TabButtons have Canvas components so they can be sorted individually
        foreach (var btn in tabButtons)
        {
            if (btn != null)
            {
                if (btn.GetComponent<Canvas>() == null) btn.gameObject.AddComponent<Canvas>();
                if (btn.GetComponent<UnityEngine.UI.GraphicRaycaster>() == null) btn.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }
        }

        // Find all Canvas components including inactive ones under the panelRoot
        Canvas[] allCanvases = panelRoot.GetComponentsInChildren<Canvas>(true);

        foreach (Canvas c in allCanvases)
        {
            // Ensure every Canvas has a GraphicRaycaster so it can receive events
            if (c.GetComponent<UnityEngine.UI.GraphicRaycaster>() == null)
            {
                c.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }

            // Check if this Canvas belongs to an ItemSelection object OR is part of a TabButton hierarchy
            if (c.GetComponent<ItemSelection>() != null || c.GetComponentInParent<TabButton>() != null)
            {
                c.enabled = true; // Ensure it's enabled
                c.overrideSorting = true;
                c.sortingOrder = contentSortingOrder + 3; // Items and TabButtons are highest (102)
            }
            else
            {
                // For all other structural parts
                c.overrideSorting = true;
                
                // Check for special groups (Backgrounds, Tabs, Buttons) -> 101
                if (IsChildOfNameContains(c.transform, "background") || 
                    IsChildOfName(c.transform, "TabGroup") 
                    // || IsChildOfNameContains(c.transform, "ItemSelectionButton") ||
                    // IsChildOfNameContains(c.transform, "ChildButton")
                    )
                {
                    c.sortingOrder = contentSortingOrder + 2; 
                }
                else
                {
                    // Default/Root -> 100
                    if (c.gameObject == panelRoot)
                    {
                        c.sortingOrder = contentSortingOrder;
                    }
                    else
                    {
                        c.sortingOrder = contentSortingOrder; 
                    }
                }
            }
        }
    }

    private bool IsChildOfName(Transform current, string targetName)
    {
        // Traverse up to panelRoot or null
        while (current != null && current != panelRoot.transform.parent) // Stop if we go past panelRoot
        {
            if (current.name == targetName) return true;
            if (current == panelRoot.transform) break; // Don't go past panelRoot
            current = current.parent;
        }
        return false;
    }

    private bool IsChildOfNameContains(Transform current, string partialName)
    {
        partialName = partialName.ToLower();
        // Traverse up to panelRoot or null
        while (current != null && current != panelRoot.transform.parent)
        {
            if (current.name.ToLower().Contains(partialName)) return true;
            if (current == panelRoot.transform) break;
            current = current.parent;
        }
        return false;
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
}
