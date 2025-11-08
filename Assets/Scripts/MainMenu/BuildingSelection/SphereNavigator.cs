using UnityEngine;
using System.Collections;

public class SphereNavigator : MonoBehaviour
{
    [Header("Template Alanları")]
    [SerializeField] private Transform[] templateAreas;
    [SerializeField] private float transitionDuration = 1f;
    [SerializeField] private AnimationCurve transitionCurve;

    [SerializeField] private CameraFollower cameraFollower;
    [Header("Otomatik çekiyor boş kalacak")]
    [SerializeField] private Canvas[] templateCanvases;


    private int currentIndex = 0;
    private bool isTransitioning = false;

    private void Start()
    {
        // Başlangıçta ilk template'i aktif konuma getir
        transform.position = templateAreas[currentIndex].position;
    }

    public void GoToNextTemplate()
    {
        if (isTransitioning || templateAreas.Length < 2) return;

        int nextIndex = (currentIndex + 1) % templateAreas.Length;
        StartCoroutine(TransitionTo(nextIndex));
    }

    public void GoToPreviousTemplate()
    {
        if (isTransitioning || templateAreas.Length < 2) return;

        int prevIndex = (currentIndex - 1 + templateAreas.Length) % templateAreas.Length;
        StartCoroutine(TransitionTo(prevIndex));
    }


    private IEnumerator TransitionTo(int targetIndex)
    {
        Debug.Log("Ilgili template="+templateAreas[targetIndex].name);
        isTransitioning = true;

        Vector3 startPos = transform.position;
        Vector3 endPos = templateAreas[targetIndex].position;

        float time = 0f;

        while (time < transitionDuration)
        {
            float t = transitionCurve.Evaluate(time / transitionDuration);
            transform.position = Vector3.Lerp(startPos, endPos, t);
            time += Time.deltaTime;
            yield return null;
        }

        transform.position = endPos;
        currentIndex = targetIndex;
        isTransitioning = false;

        // 🎯 Kamera hedefini güncelle
        if (cameraFollower != null)
            cameraFollower.SetTarget(templateAreas[currentIndex]);

        // 🎯 Canvas'ları güncelle (dinamik)
        for (int i = 0; i < templateAreas.Length; i++)
        {
            Canvas canvas = templateAreas[i].GetComponentInChildren<Canvas>(true);
            if (canvas != null)
                canvas.enabled = (i == currentIndex);
        }
    }

}