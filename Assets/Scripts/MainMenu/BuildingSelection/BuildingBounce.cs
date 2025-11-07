using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class BuildingBounce : MonoBehaviour
{
    [SerializeField] private float bounceHeight = 20f;
    [SerializeField] private float bounceScale = 1.2f;
    [SerializeField] private float bounceDuration = 0.4f;

    private RectTransform rt;
    private Vector2 originalPos;
    private Vector3 originalScale;

    private void Awake()
    {
        rt = GetComponent<RectTransform>();
        if (rt != null)
            originalPos = rt.anchoredPosition;

        originalScale = transform.localScale;
    }

    public void BounceOnce()
    {
        StopAllCoroutines();
        StartCoroutine(BounceRoutine());
    }

    private IEnumerator BounceRoutine()
    {
        float t = 0f;

        while (t < bounceDuration)
        {
            t += Time.deltaTime;
            float normalized = t / bounceDuration;

            float bounce = Mathf.Sin(normalized * Mathf.PI);
            rt.anchoredPosition = originalPos + Vector2.up * bounce * bounceHeight;
            transform.localScale = originalScale * (1f + bounce * (bounceScale - 1f));

            yield return null;
        }

        rt.anchoredPosition = originalPos;
        transform.localScale = originalScale;
    }
}
