using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemSelectionPanelController : MonoBehaviour
{
    public static ItemSelectionPanelController Instance;

    [SerializeField] private GameObject panelRoot;
    [SerializeField] private GameObject tabGroup; 
    [SerializeField] private List<TabButton> tabButtons;
    [SerializeField] private List<GameObject> tabContents;
    public List<GameObject> TabContents => tabContents;

    [Header("Visibility Limits")]
    [SerializeField] private Transform topLimit;
    [SerializeField] private Transform bottomLimit;
    [SerializeField] private Transform leftLimit; 
    [SerializeField] private Transform rightLimit; 
    public Transform TopLimit => topLimit;
    public Transform BottomLimit => bottomLimit;
    public Transform LeftLimit => leftLimit; 
    public Transform RightLimit => rightLimit; 

    private bool isActive = false;
    private int currentTabIndex = 0; // Track active tab for filtering

    private AutoLoadTabContents autoLoadTabContents;
    private bool isActiveLoadTabContents = false;

    [SerializeField] private int panelSortingOrder = 100; 
    [SerializeField] private int contentSortingOrder = 101; 
    public int ContentSortingOrder => contentSortingOrder;

    private List<Vector3> tabButtonInitialScales;
    private List<Canvas> _cachedItemCanvases = new List<Canvas>();
    private List<Canvas> _cachedTabCanvases = new List<Canvas>();

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
    
    private void LoadTabContents()
    {
        if(isActiveLoadTabContents!=true)
        {
            // Trigger automatic loading of tab contents
            autoLoadTabContents= GetComponent<AutoLoadTabContents>();
            if (autoLoadTabContents == null) autoLoadTabContents = FindObjectOfType<AutoLoadTabContents>();
            
            if (autoLoadTabContents != null)
            {
                autoLoadTabContents.LoadAllTabs();
            }
            else
            {
                Debug.LogWarning("[ItemSelectionPanelController] AutoLoadTabContents script not found in scene.");
            }
            isActiveLoadTabContents = true;
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
        LoadTabContents();

        isActive = true; 
        panelRoot.SetActive(true);

        // Ensure the panel renders on top of other Canvas elements
        Canvas canvas = panelRoot.GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = panelRoot.AddComponent<Canvas>();
            panelRoot.AddComponent<GraphicRaycaster>();
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

                ScrollRect scrollRect = contentObj.GetComponent<ScrollRect>();
                if (scrollRect != null)
                {
                    if (scrollRect.content != null) EnsureTransparentImage(scrollRect.content.gameObject);
                    if (scrollRect.viewport != null) EnsureTransparentImage(scrollRect.viewport.gameObject);

                }
            }
        }

        // Fix for scroll dead zones (TabGroup)
        if (tabGroup != null)
        {
            ScrollRect tabScroll = tabGroup.GetComponent<ScrollRect>();
            if (tabScroll != null)
            {
                EnsureTransparentImage(tabGroup); 
                if (tabScroll.content != null) EnsureTransparentImage(tabScroll.content.gameObject);
                if (tabScroll.viewport != null) EnsureTransparentImage(tabScroll.viewport.gameObject);
            }
        }

        // Apply sorting order to ALL canvases under panelRoot (Recursive)
        //ApplySortingOrderToAll();

        SelectTab(0); // Default first tab

        // Force UI update and register events immediately
        ForceUpdateVisibility();
        StartCoroutine(RefreshUIRoutine());
    }

    public void ClosePanel()
    {
        isActive = false;
        panelRoot.SetActive(false);
    }

    public void SelectTab(int index)
    {
        Debug.Log("SelectTab called");
        currentTabIndex = index;
        // 1. Ensure all tabs are active (to prevent state loss)
        for (int i = 0; i < tabContents.Count; i++)
        {
            if (tabContents[i] != null && !tabContents[i].activeSelf)
            {
                tabContents[i].SetActive(true);
            }
        }

        // 2. Reset everything to default visibility
        ApplySortingOrderToAll();

        // 3. Update CanvasGroups for visibility and interactivity
        for (int i = 0; i < tabContents.Count; i++)
        {
            if (tabContents[i] != null)
            {
                CanvasGroup cg = EnsureCanvasGroup(tabContents[i]);
                if (i == index)
                {
                    // Selected Tab: Visible and Interactable
                    cg.alpha = 1f;
                    cg.interactable = true;
                    cg.blocksRaycasts = true;
                }
                else
                {
                     // Inactive Tabs: Hidden and Non-interactable
                    cg.alpha = 0f;
                    cg.interactable = false;
                    cg.blocksRaycasts = false;
                    
                    // Also lower sorting order as a backup
                    Canvas[] childCanvases = tabContents[i].GetComponentsInChildren<Canvas>(true);
                    foreach (var c in childCanvases)
                    {
                        c.sortingOrder = -50;
                    }
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
        ForceUpdateVisibility();
        StartCoroutine(RefreshUIRoutine());
    }

    private CanvasGroup EnsureCanvasGroup(GameObject obj)
    {
        CanvasGroup cg = obj.GetComponent<CanvasGroup>();
        if (cg == null)
        {
            cg = obj.AddComponent<CanvasGroup>();
        }
        return cg;
    }

    public void ForceUpdateVisibility()
    {
        if (panelRoot == null) return;

        // Force rebuild
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(panelRoot.GetComponent<RectTransform>());

        // --- CACHE LISTS FOR PERFORMANCE ---
        _cachedItemCanvases.Clear();
        _cachedTabCanvases.Clear();

        // Cache ItemSelection Canvases
        var items = panelRoot.GetComponentsInChildren<ItemSelection>(true);
        foreach (var item in items)
        {
            Canvas c = item.GetComponent<Canvas>();
            if (c == null)
            {
                c = item.gameObject.AddComponent<Canvas>();
                // Also ensure GraphicRaycaster if we are adding Canvas
                if (item.GetComponent<GraphicRaycaster>() == null)
                {
                    item.gameObject.AddComponent<GraphicRaycaster>();
                }
            }

            _cachedItemCanvases.Add(c);
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
        // -----------------------------------

        RegisterScrollEvents();
        CheckVisibility();
    }

    private IEnumerator RefreshUIRoutine()
    {
        // Wait for end of frame to ensure all SetActive/Layout calculations are done
        yield return new WaitForEndOfFrame();
        
        ForceUpdateVisibility();
    }

    private void RegisterScrollEvents()
    {
        // Add listener to TabGroup ScrollRect
        if (tabGroup != null)
        {
            var sr = tabGroup.GetComponent<ScrollRect>();
            if (sr != null)
            {
                sr.onValueChanged.RemoveListener(OnScrollValueChanged);
                sr.onValueChanged.AddListener(OnScrollValueChanged);
            }
        }

        // Add listener to all TabContent ScrollRects
        foreach (var contentObj in tabContents)
        {
            if (contentObj != null)
            {
                var sr = contentObj.GetComponent<ScrollRect>();
                if (sr != null)
                {
                    sr.onValueChanged.RemoveListener(OnScrollValueChanged);
                    sr.onValueChanged.AddListener(OnScrollValueChanged);
                }
            }
        }
    }

    private void OnScrollValueChanged(Vector2 val)
    {
        CheckVisibility();
    }

    private void CheckVisibility()
    {
        if (panelRoot == null) return;

        float visibilityBuffer = 0f; // Buffer to account for item width

        // Pre-calculate Limit positions in Panel Space
        float topY = (topLimit != null) ? panelRoot.transform.InverseTransformPoint(topLimit.position).y : float.MaxValue;
        float bottomY = (bottomLimit != null) ? panelRoot.transform.InverseTransformPoint(bottomLimit.position).y : float.MinValue;
        float leftX = (leftLimit != null) ? panelRoot.transform.InverseTransformPoint(leftLimit.position).x : float.MinValue;
        float rightX = (rightLimit != null) ? panelRoot.transform.InverseTransformPoint(rightLimit.position).x : float.MaxValue;

        // 1. Check ItemSelections (Vertical)
        foreach (Canvas c in _cachedItemCanvases)
        {
            if (c == null) continue;
            
            // USER REQUEST FIX: Force items in inactive tabs to stay hidden/blocked
            // This prevents "Ghost Clicks" from items in hidden tabs that happen to be in the visible Y-range
            if (currentTabIndex < tabContents.Count && tabContents[currentTabIndex] != null)
            {
                 if (!c.transform.IsChildOf(tabContents[currentTabIndex].transform))
                 {
                     // Force Hide if it's not already hidden
                     if (c.sortingOrder == -50)
                     {
                         c.overrideSorting = true;
                         c.sortingOrder = -50;
                         var cg = EnsureCanvasGroup(c.gameObject);
                         cg.blocksRaycasts = false;
                     }
                     // Skip visibility calculation for this item entirely
                     continue; 
                 }
            }
            
            bool isVisible = true;
            // Convert Item position to Panel Space
            Vector3 localPos = panelRoot.transform.InverseTransformPoint(c.transform.position);

            if (localPos.y > topY + visibilityBuffer) isVisible = false;
            if (localPos.y < bottomY - visibilityBuffer) isVisible = false;

            if (isVisible)
            {
                if (c.sortingOrder == -50)
                {
                    c.overrideSorting = true;
                    c.sortingOrder = contentSortingOrder + 3;
                    // USER REQUEST: Enable interaction for ITEMS when visible
                    var cg = EnsureCanvasGroup(c.gameObject);
                    cg.blocksRaycasts = false;
                }
                else
                {
                    var cg = EnsureCanvasGroup(c.gameObject);
                    cg.blocksRaycasts = true;
                }
            }
            else
            {
                if (c.sortingOrder != -50)
                {
                    c.overrideSorting = true;
                    c.sortingOrder = -50;
                    var cg = EnsureCanvasGroup(c.gameObject);
                    cg.blocksRaycasts = true;
                }
            }
        }

        // 2. Check TabButtons (Horizontal)
        foreach (Canvas c in _cachedTabCanvases)
        {
            if (c == null) continue;

            bool isVisible = true;
            // Convert TabButton position to Panel Space
            Vector3 localPos = panelRoot.transform.InverseTransformPoint(c.transform.position);

            if (localPos.x < leftX - visibilityBuffer) isVisible = false;
            if (localPos.x > rightX + visibilityBuffer) isVisible = false;

            if (isVisible)
            {
                if (c.sortingOrder == -50)
                {
                    c.overrideSorting = true;
                    c.sortingOrder = contentSortingOrder + 3;
                }
            }
            else
            {
                if (c.sortingOrder != -50)
                {
                    c.overrideSorting = true;
                    c.sortingOrder = -50;
                }
            }
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
                if (btn.GetComponent<GraphicRaycaster>() == null) btn.gameObject.AddComponent<GraphicRaycaster>();
            }
        }

        // Find all Canvas components including inactive ones under the panelRoot
        Canvas[] allCanvases = panelRoot.GetComponentsInChildren<Canvas>(true);

        foreach (Canvas c in allCanvases)
        {
            // Ensure every Canvas has a GraphicRaycaster so it can receive events
            if (c.GetComponent<GraphicRaycaster>() == null)
            {
                c.gameObject.AddComponent<GraphicRaycaster>();
            }

            // Check if this Canvas belongs to an ItemSelection object
            if (c.GetComponent<ItemSelection>() != null)
            {
                c.enabled = true; // Ensure it's enabled
                c.overrideSorting = true;
                c.sortingOrder = -50; 
                
                // USER REQUEST: Disable interaction for ITEMS initially
                var cg = EnsureCanvasGroup(c.gameObject);
                cg.blocksRaycasts = false;
            }
            // Check if it is part of a TabButton hierarchy (Do NOT disable interaction)
            else if (c.GetComponentInParent<TabButton>() != null)
            {
                c.enabled = true;
                c.overrideSorting = true;
                c.sortingOrder = -50;
                
                // TabButtons should stay interactive even if technically hidden sorting-wise?
                // Or maybe the user just doesn't want me to explicit set it to false.
                // Leaving it as default (likely true).
            }
            else
            {
                // For all other structural parts
                c.overrideSorting = true;
                
                // Check for special groups (Backgrounds, Tabs, Buttons) -> 101
                if (IsChildOfNameContains(c.transform, "background") || 
                    IsChildOfName(c.transform, "TabGroup"))
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
        Image img = obj.GetComponent<Image>();
        if (img == null)
        {
            img = obj.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0.004f);
        }
        img.raycastTarget = true;
    }
}
