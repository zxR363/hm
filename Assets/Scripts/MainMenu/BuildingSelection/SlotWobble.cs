using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Slotların(Binaların,Gameobjectlerin) salınım efektini yapıyor
public class SlotWobble : MonoBehaviour
{
    [SerializeField] private float wobbleSpeed = 2f;
    [SerializeField] private float wobbleAmount = 5f;

    private Vector3 initialRotation;

    private void Start()
    {
        initialRotation = transform.localEulerAngles;
    }

    private void Update()
    {
        float wobble = Mathf.Sin(Time.time * wobbleSpeed) * wobbleAmount;
        transform.localEulerAngles = initialRotation + new Vector3(0f, 0f, wobble);
    }
}
