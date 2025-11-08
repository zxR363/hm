using UnityEngine;
using System.Collections;

public class SlotWobble : MonoBehaviour
{
    [SerializeField] private float wobbleSpeed = 20f;
    [SerializeField] private float wobbleAmount = 5f;
    private float wobbleDuration = 3f;

    private Vector3 initialRotation;
    private Coroutine wobbleRoutine;

    private void Awake()
    {
        initialRotation = transform.localEulerAngles;
    }

    public void TriggerWobble()
    {
        if (wobbleRoutine != null)
            StopCoroutine(wobbleRoutine);

        wobbleRoutine = StartCoroutine(WobbleOnce());
    }

    private IEnumerator WobbleOnce()
    {
        float time = 0f;

        while (time < wobbleDuration)
        {
            float wobble = Mathf.Sin(time * wobbleSpeed) * wobbleAmount;
            transform.localEulerAngles = initialRotation + new Vector3(0f, 0f, wobble);
            time += Time.deltaTime;
            yield return null;
        }

        transform.localEulerAngles = initialRotation;
        wobbleRoutine = null;
    }
}