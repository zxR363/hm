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
    
        private int currentIndex = 0;
    private bool forward = true;
    private Tween currentTween;


    private void Start()
    {
        // Başlangıç pozisyonunu anında uygula
        Vector2 start = localWaypointPositions[0];
        transform.localPosition = new Vector3(start.x, start.y, fixedLocalZ);

        currentIndex = 0;
        forward = true;

        TriggerAnimations();
    }

    private void OnDisable()
    {
        if (moveSequence != null && moveSequence.IsActive())
            moveSequence.Kill();
    }

    public void TriggerAnimations()
    {
        if (localWaypointPositions.Count < 2)
        {
            Debug.LogWarning($"[{name}] ShipSmoothMover: En az 2 local waypoint gerekli.");
            return;
        }

        StartNextTween();
    }

    private void StartNextTween()
    {
        int nextIndex = forward ? currentIndex + 1 : currentIndex - 1;

        // Sınır kontrolü
        if (nextIndex >= localWaypointPositions.Count)
        {
            forward = false;
            nextIndex = currentIndex - 1;
        }
        else if (nextIndex < 0)
        {
            forward = true;
            nextIndex = currentIndex + 1;
        }

        Vector2 nextPos = localWaypointPositions[nextIndex];
        Vector3 target = new Vector3(nextPos.x, nextPos.y, fixedLocalZ);

        currentTween = transform.DOLocalMove(target, moveDuration)
            .SetEase(Ease.Linear)
            .SetSpeedBased(false)
            .SetUpdate(UpdateType.Normal, true)
            .OnComplete(() =>
            {
                currentIndex = nextIndex;
                StartNextTween(); // zincirleme
            });
    }

}