using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    [SerializeField] private Transform cameraRig;
    [SerializeField] private bool lockY = true;

    private void LateUpdate()
    {
        if (cameraRig == null) return;

        Vector3 direction = (transform.position- cameraRig.position ).normalized;

        if (lockY)
            direction.y = 0f;

        transform.rotation = Quaternion.LookRotation(direction);
    }
}