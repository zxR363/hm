using UnityEngine;
using DG.Tweening;

public class BaloonMovementBehaviour : MonoBehaviour
{
    [SerializeField] private float moveAmount = 0.5f;
    [SerializeField] private float duration = 1.5f;

    void Start()
    {
        float startY = transform.localPosition.y;

        transform.DOLocalMoveY(startY + moveAmount, duration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }
}
