using UnityEngine;
using System.Collections;

public class SphereNavigator : MonoBehaviour
{
    [Header("Template Alanları")]
    [SerializeField] private Transform[] templateAreas;
    [SerializeField] private float transitionDuration = 1f;
    [SerializeField] private AnimationCurve transitionCurve;

    private int currentIndex = 0;
    private bool isTransitioning = false;

    private void Start()
    {
        // Başlangıçta sadece ilk template aktif
        for (int i = 0; i < templateAreas.Length; i++)
            templateAreas[i].gameObject.SetActive(i == currentIndex);

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
        Debug.Log("Ilgili template = " + templateAreas[targetIndex].name);
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

        // 🎯 Template görünürlüğünü güncelle
        for (int i = 0; i < templateAreas.Length; i++)
            templateAreas[i].gameObject.SetActive(i == currentIndex);

        // 🎯 Wobble tetikle: BuildingGrid altındaki tüm child objelerde
        Transform activeTemplate = templateAreas[currentIndex];
        Transform buildingGrid = activeTemplate.Find("BuildingGrid");

        if (buildingGrid != null)
        {
            foreach (Transform child in buildingGrid)
            {
                ApplyWobbleRecursively(child);
            }
        }
    }

    private void ApplyWobbleRecursively(Transform root)
    {
        SlotWobble wobble = root.GetComponent<SlotWobble>();
        if (wobble != null)
            wobble.TriggerWobble();

        foreach (Transform child in root)
        {
            ApplyWobbleRecursively(child);
        }
    }
}