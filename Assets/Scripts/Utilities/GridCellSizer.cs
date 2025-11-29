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

    private void OnRectTransformDimensionsChange()
    {
        UpdateCellSize();
    }

    private void UpdateCellSize()
    {
        if (grid == null || rectTransform == null) return;

        float width = rectTransform.rect.width;
        if (width <= 0) return;

        // Calculate available width for cells
        float availableWidth = width - grid.padding.left - grid.padding.right - (grid.spacing.x * (columnCount - 1));
        float cellWidth = availableWidth / columnCount;

        // Apply size
        if (maintainAspectRatio)
        {
            grid.cellSize = new Vector2(cellWidth, cellWidth / aspectRatio);
        }
        else
        {
            grid.cellSize = new Vector2(cellWidth, grid.cellSize.y);
        }
    }

    private void Update()
    {
        // Optional: Check if width changed every frame if OnRectTransformDimensionsChange isn't reliable enough
        // or if parent changes size.
        // For performance, better to rely on events, but UI layout can be tricky.
        // Let's check if width changed.
        if (rectTransform.hasChanged)
        {
            UpdateCellSize();
            rectTransform.hasChanged = false;
        }
    }
}
