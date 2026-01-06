using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SnapToSlot : MonoBehaviour
{
    public ScrollRect scrollRect;
    public RectTransform content;
    public float snapSpeed = 10f;
    public float snapThreshold = 100f; // velocity threshold
    public float slotWidth = 200f;
    public int totalSlots = 7;

    private bool isSnapping = false;

    void Update()
    {
        if (scrollRect == null || content == null) return;

        if (scrollRect.velocity.magnitude < snapThreshold && !isSnapping)
        {
            // PRE-CHECK: Don't start snapping if we are already there
            float contentX = content.anchoredPosition.x;
            float targetIndex = Mathf.Round(-contentX / slotWidth);
            float targetX = -targetIndex * slotWidth;

            if (Mathf.Abs(contentX - targetX) > 0.01f) // Only snap if actually misaligned
            {
                StartCoroutine(SnapToNearestSlot());
            }
        }
    }

    private IEnumerator SnapToNearestSlot()
    {
        isSnapping = true;

        float contentX = content.anchoredPosition.x;
        float targetIndex = Mathf.Round(-contentX / slotWidth);
        targetIndex = Mathf.Clamp(targetIndex, 0, totalSlots - 1);

        float targetX = -targetIndex * slotWidth;

        while (Mathf.Abs(content.anchoredPosition.x - targetX) > 0.1f)
        {
            float newX = Mathf.Lerp(content.anchoredPosition.x, targetX, Time.deltaTime * snapSpeed);
            content.anchoredPosition = new Vector2(newX, content.anchoredPosition.y);
            yield return null;
        }

        // Final Snap: Only set if different
        if (Mathf.Abs(content.anchoredPosition.x - targetX) > 0.001f)
        {
            content.anchoredPosition = new Vector2(targetX, content.anchoredPosition.y);
        }
        
        isSnapping = false;
    }
}