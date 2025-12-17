using UnityEngine;
using UnityEngine.Rendering;

public class UniversalDepthSorter : MonoBehaviour
{
    private enum SortTarget { None, Canvas, SpriteRenderer, SortingGroup }
    private SortTarget _targetType = SortTarget.None;

    private Canvas _canvas;
    private SpriteRenderer _spriteRenderer;
    private SortingGroup _sortingGroup;

    [Header("Settings")]
    [Tooltip("Multiplier for Y position. Higher = More sensitivity.")]
    [SerializeField] private float sortingFactor = 1.0f; 

    [Tooltip("Add this to the calculated order.")]
    [SerializeField] private int baseOrder = 0;

    [Tooltip("Manual Y Offset to adjust 'Feet' position.")]
    [SerializeField] private float yOffset = 0f;

    [Tooltip("If true, runs every frame. Disable for static objects.")]
    [SerializeField] private bool runEveryFrame = true;

    private void Awake()
    {
        // Prioritize SortingGroup (Handles everything under it)
        _sortingGroup = GetComponent<SortingGroup>();
        if (_sortingGroup != null)
        {
            _targetType = SortTarget.SortingGroup;
            return;
        }

        // Then Canvas (UI overrides)
        _canvas = GetComponent<Canvas>();
        if (_canvas != null)
        {
            _targetType = SortTarget.Canvas;
            return;
        }

        // Finally SpriteRenderer
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer != null)
        {
            _targetType = SortTarget.SpriteRenderer;
            return;
        }
    }

    private void LateUpdate()
    {
        if (runEveryFrame)
        {
            UpdateSorting();
        }
    }

    public void UpdateSorting()
    {
        if (_targetType == SortTarget.None) return;

        // Calculate "Feet" Y
        // For simple objects, position.y is pivot.
        float y = transform.position.y + yOffset;

        // Logic: Screen Top (High Y) = Behind (Low Order)
        //        Screen Bottom (Low Y) = Front (High Order)
        
        // E.g. Y=100 -> Order = -100
        //      Y=-100 -> Order = 100
        
        int order = baseOrder - Mathf.RoundToInt(y * sortingFactor);

        switch (_targetType)
        {
            case SortTarget.Canvas:
                if (_canvas.sortingOrder != order)
                {
                    _canvas.overrideSorting = true;
                    _canvas.sortingOrder = order;
                }
                break;
                
            case SortTarget.SortingGroup:
                if (_sortingGroup.sortingOrder != order)
                    _sortingGroup.sortingOrder = order;
                break;

            case SortTarget.SpriteRenderer:
                if (_spriteRenderer.sortingOrder != order)
                    _spriteRenderer.sortingOrder = order;
                break;
        }
    }
}
