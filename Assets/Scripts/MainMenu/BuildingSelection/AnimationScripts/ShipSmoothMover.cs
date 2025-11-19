using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class ShipSmoothMover : MonoBehaviour
{
    [Header("Hedef Noktalar (local posX, posY)")]
    public List<Vector2> localWaypointPositions = new(); // Inspector'dan girilecek

    [Header("Tween Ayarları")]
    public float moveDuration = 1f;
    public float fixedLocalZ = 0f; // local Z sabit

    private Sequence moveSequence;

    private void Start()
    {
        if (localWaypointPositions.Count < 2)
        {
            Debug.LogWarning("En az 2 local waypoint gerekli.");
            return;
        }

        CreateLoopSequence();
    }

    private void CreateLoopSequence()
    {
        moveSequence = DOTween.Sequence();

        // 🔁 1 → 2 → 3 → 4
        foreach (Vector2 point in localWaypointPositions)
        {
            Vector3 targetLocalPos = new Vector3(point.x, point.y, fixedLocalZ);
            moveSequence.Append(transform.DOLocalMove(targetLocalPos, moveDuration).SetEase(Ease.Linear));
        }

        // 🔁 4 → 3 → 2 → 1
        for (int i = localWaypointPositions.Count - 2; i >= 0; i--)
        {
            Vector3 targetLocalPos = new Vector3(localWaypointPositions[i].x, localWaypointPositions[i].y, fixedLocalZ);
            moveSequence.Append(transform.DOLocalMove(targetLocalPos, moveDuration).SetEase(Ease.Linear));
        }

        moveSequence.SetLoops(-1); // sonsuz döngü
    }

    private void OnDisable()
    {
        if (moveSequence != null && moveSequence.IsActive())
            moveSequence.Kill();
    }
}