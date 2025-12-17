using UnityEngine;
using UnityEngine.UI;

public class DragAutoScroller : MonoBehaviour
{
    public static DragAutoScroller Instance { get; private set; }

    [Header("Target")]
    [Tooltip("Assign the Main Room ScrollRect here.")]
    public ScrollRect targetScrollRect;

    [Header("Settings")]
    [Tooltip("Distance from screen edge to trigger SCROLL.")]
    [SerializeField] private float edgeThreshold = 100f;
    
    [Tooltip("Scroll speed multiplier.")]
    [SerializeField] private float scrollSpeed = 500f; // Pixels per second

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// Call this from OnDrag methods.
    /// </summary>
    /// <param name="pointerScreenPos">Input.mousePosition or eventData.position</param>
    public void ProcessDrag(Vector2 pointerScreenPos)
    {
        if (targetScrollRect == null) return;

        // Reset velocity to ensure smooth manual control, 
        // or add to it? Usually setting normalized position is smoother for "Pushing",
        // but ScrollRect.velocity is better for physics. Let's try velocity.
        
        Vector2 velocity = Vector2.zero;
        
        // Check Horizontal Edges
        if (pointerScreenPos.x < edgeThreshold)
        {
            // Left Edge -> Scroll Right (Content moves Right to show Left)
            // Wait, ScrollRect velocity: positive x moves content right.
            velocity.x = scrollSpeed; 
        }
        else if (pointerScreenPos.x > Screen.width - edgeThreshold)
        {
            // Right Edge -> Scroll Left
            velocity.x = -scrollSpeed;
        }

        // Check Vertical (Optional, usually for Rooms it's Horizontal)
        // If needed:
        // if (pointerScreenPos.y < edgeThreshold) velocity.y = scrollSpeed;
        // else if (pointerScreenPos.y > Screen.height - edgeThreshold) velocity.y = -scrollSpeed;

        if (velocity != Vector2.zero)
        {
            // Apply smoothly
            targetScrollRect.velocity = Vector2.Lerp(targetScrollRect.velocity, velocity, Time.deltaTime * 10f);
        }
    }
}
