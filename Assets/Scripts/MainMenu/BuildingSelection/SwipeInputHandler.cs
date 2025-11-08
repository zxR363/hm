using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwipeInputHandler : MonoBehaviour
{
    [Header("Bağlantılar")]
    [SerializeField] private SphereNavigator navigator;

    [Header("Swipe Ayarları")]
    [SerializeField] private float swipeThreshold = 50f;

    private Vector2 startPos;
    private bool isSwiping = false;

    private void Update()
    {
        // 🖱 Mouse swipe desteği
        if (Input.GetMouseButtonDown(0))
        {
            startPos = Input.mousePosition;
            isSwiping = true;
        }
        else if (Input.GetMouseButtonUp(0) && isSwiping)
        {
            Vector2 endPos = Input.mousePosition;
            Vector2 delta = endPos - startPos;

            if (Mathf.Abs(delta.x) > swipeThreshold)
            {
                if (delta.x > 0)
                    navigator.GoToPreviousTemplate();
                else
                    navigator.GoToNextTemplate();
            }

            isSwiping = false;
        }

        // 📱 Touch swipe desteği (mobil)
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                startPos = touch.position;
                isSwiping = true;
            }
            else if (touch.phase == TouchPhase.Ended && isSwiping)
            {
                Vector2 endPos = touch.position;
                Vector2 delta = endPos - startPos;

                if (Mathf.Abs(delta.x) > swipeThreshold)
                {
                    if (delta.x > 0)
                        navigator.GoToPreviousTemplate();
                    else
                        navigator.GoToNextTemplate();
                }

                isSwiping = false;
            }
        }
    }
}
