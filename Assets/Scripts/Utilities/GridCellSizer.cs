using UnityEngine;
using UnityEngine.UI;

/*
Bu script, ekran genişliği veya panel boyutu ne olursa olsun,
 hücreleri (cell) otomatik olarak küçültüp büyüterek o alana 
 tam sığdıracaktır. Böylece yatay taşma (horizontal overflow) yaşamazsınız.
*/
[RequireComponent(typeof(GridLayoutGroup))]
public class GridCellSizer : MonoBehaviour
{
    private GridLayoutGroup grid;
    private RectTransform rectTransform;

    [SerializeField] private int columnCount = 3;
    [SerializeField] private bool maintainAspectRatio = true;
    [SerializeField] private float aspectRatio = 1f; // Width / Height

    private void Awake()
    {
        grid = GetComponent<GridLayoutGroup>();
        rectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        UpdateCellSize();
    }

    private float lastWidth;

    private void OnRectTransformDimensionsChange()
    {
        // UpdateCellSize(); // Disabled: Causes recursive layout rebuilds
    }

    private void UpdateCellSize()
    {
        if (grid == null || rectTransform == null) return;

        float width = rectTransform.rect.width;
        if (width <= 0) return;

        // Apply size
        // ... calculation logic ...
        float availableWidth = width - grid.padding.left - grid.padding.right - (grid.spacing.x * (columnCount - 1));
        float cellWidth = availableWidth / columnCount;

        if (maintainAspectRatio)
        {
            Vector2 newSize = new Vector2(cellWidth, cellWidth / aspectRatio);
            if ((grid.cellSize - newSize).sqrMagnitude > 0.01f)
                grid.cellSize = newSize;
        }
        else
        {
            Vector2 newSize = new Vector2(cellWidth, grid.cellSize.y);
            if ((grid.cellSize - newSize).sqrMagnitude > 0.01f)
                grid.cellSize = newSize;
        }
    }

    private void Update()
    {
        if (rectTransform == null) return;
        
        // Safer check: Only update if width actually changed
        if (Mathf.Abs(rectTransform.rect.width - lastWidth) > 1f)
        {
            lastWidth = rectTransform.rect.width;
            UpdateCellSize();
        }
    }
}
