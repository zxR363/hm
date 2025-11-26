using UnityEngine;

public class ClockMovementBehaviour : MonoBehaviour
{
    [Header("Rotation Settings")]
    [Tooltip("Saniyedeki dönüş hızı (derece)")]
    public float rotationSpeed = 60f; 
    
    [Tooltip("Saat yönünde mi dönecek?")]
    public bool clockwise = true;

    void Update()
    {
        // Saat yönü için eksi (-), tersi için artı (+)
        float direction = clockwise ? -1f : 1f;
        
        // Z ekseninde döndür (Yelkovan efekti için)
        transform.Rotate(0, 0, direction * rotationSpeed * Time.deltaTime);
    }
}
