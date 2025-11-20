using UnityEngine;
using UnityEngine.UI;

public class FallingSpriteItem : MonoBehaviour, IPoolable
{
    public void OnReturnToPool()
    {
        Image img = GetComponent<Image>();
        if (img != null)
        {
            img.sprite = null;
            img.enabled = false; // 👈 sprite yoksa görünmesin
        }

        RectTransform rt = GetComponent<RectTransform>();
        if (rt != null)
            rt.anchoredPosition = Vector2.zero;

        transform.localScale = Vector3.one;
        // DOTween.Kill(gameObject);

    }
}