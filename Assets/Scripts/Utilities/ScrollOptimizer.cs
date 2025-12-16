using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class ScrollOptimizer : MonoBehaviour
{
    [Header("Optimization Settings")]
    [Tooltip("Control how fast the content slows down. Higher = Smoother/Faster feel (0.135 is default)")]
    [SerializeField] private float decelerationRate = 0.35f; 
    
    [Tooltip("Control sensitivity for Mouse Wheel / Trackpad")]
    [SerializeField] private float scrollSensitivity = 50f;

    private void Awake()
    {
        ApplySettings();
    }

    private void OnValidate()
    {
        ApplySettings();
    }

    public void ApplySettings()
    {
        ScrollRect scrollRect = GetComponent<ScrollRect>();
        if (scrollRect != null)
        {
            scrollRect.decelerationRate = decelerationRate;
            scrollRect.scrollSensitivity = scrollSensitivity;
        }
    }
}
