using UnityEngine;

public class CharacterState : MonoBehaviour
{
    public EnumCharacterMood currentMood = EnumCharacterMood.Neutral;
    public Animator animator;

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    public void SetMood(EnumCharacterMood mood)
    {
        currentMood = mood;
        if(animator != null)
        {
            animator.SetTrigger(mood.ToString()); // Örn: "Happy" trigger'ı
        }
        Debug.Log($"{gameObject.name} ruh hali: {mood}");
    }
}
