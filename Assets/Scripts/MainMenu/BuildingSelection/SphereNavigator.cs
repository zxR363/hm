using UnityEngine;
using System.Collections;

public class SphereNavigator : MonoBehaviour
{
    [Header("Template Alanları")]
    [SerializeField] private Transform[] templateAreas;
    [SerializeField] private float transitionDuration = 1f;
    [SerializeField] private AnimationCurve transitionCurve;

    [Header("Kamera Takibi")]
    [SerializeField] private CameraFollower cameraFollower;

    private int currentIndex = 0;
    private bool isTransitioning = false;

    private void Start()
    {
        transform.rotation = Quaternion.LookRotation(templateAreas[currentIndex].position - transform.position);
        if (cameraFollower != null)
            cameraFollower.SetTarget(templateAreas[currentIndex]);
        UpdateCanvasStates();
    }

    public void GoToNextTemplate()
    {
        if (isTransitioning || templateAreas.Length < 2) return;
        int nextIndex = (currentIndex + 1) % templateAreas.Length;
        StartCoroutine(RotateToTemplate(nextIndex));
    }

    public void GoToPreviousTemplate()
    {
        if (isTransitioning || templateAreas.Length < 2) return;
        int prevIndex = (currentIndex - 1 + templateAreas.Length) % templateAreas.Length;
        StartCoroutine(RotateToTemplate(prevIndex));
    }

    private IEnumerator RotateToTemplate(int targetIndex)
    {
        isTransitioning = true;

        Quaternion startRot = transform.rotation;
        Quaternion endRot = Quaternion.LookRotation(templateAreas[targetIndex].position - transform.position);

        float time = 0f;
        while (time < transitionDuration)
        {
            float t = transitionCurve.Evaluate(time / transitionDuration);
            transform.rotation = Quaternion.Slerp(startRot, endRot, t);
            time += Time.deltaTime;
            yield return null;
        }

        transform.rotation = endRot;
        currentIndex = targetIndex;
        isTransitioning = false;

        if (cameraFollower != null)
            cameraFollower.SetTarget(templateAreas[currentIndex]);

        UpdateCanvasStates();
    }

    private void UpdateCanvasStates()
    {
        for (int i = 0; i < templateAreas.Length; i++)
        {
            Canvas canvas = templateAreas[i].GetComponentInChildren<Canvas>(true);
            if (canvas != null)
                canvas.enabled = (i == currentIndex);
        }
    }
}