using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwipeInputHandler : MonoBehaviour
{
    [Header("Bağlantılar")]
    [SerializeField] private SphereNavigator navigator;

    [Header("Swipe Ayarları")]
    [SerializeField] private float swipeThreshold = 50f;

    [Header("Geçiş Butonları")]
    public Button leftButton;
    public Button rightButton;

    private Vector2 startPos;
    private bool isSwiping = false;

    private void Start()
    {
        leftButton.onClick.AddListener(GoLeft);
        rightButton.onClick.AddListener(GoRight);
    }

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


    private void GoLeft()
    {
        StartCoroutine(TransitionToPage(leftButton));
    }

    private void GoRight()
    {
        StartCoroutine(TransitionToPage(rightButton));
    }


    private IEnumerator TransitionToPage(Button selectedButton)
    {
        // 🔥 Buton scale efekti
        Button clickedButton = selectedButton;
        Transform buttonVisual = clickedButton.transform;
        Vector3 originalScale = buttonVisual.localScale;
        buttonVisual.localScale = originalScale * 0.7f;

        float duration = 0.15f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 🔄 Buton scale geri dönüş
        buttonVisual.localScale = originalScale;
        
        if (clickedButton.name == leftButton.name)
        {
            Debug.Log("LEFT BUTONA TIKLADIK");
            navigator.GoToPreviousTemplate();
        }
        else if(clickedButton.name == rightButton.name)
        {
            Debug.Log("RIGHT BUTONA TIKLADIK");
            navigator.GoToNextTemplate();        
        }           
    }
}
