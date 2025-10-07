using UnityEngine;
using UnityEngine.UI;

/*
 *   Özellik	Açmak için ne yapılmalı?
 *   Animasyon	enableAnimation = true + Animator + trigger adı
 *   Ses efekti	enableSound = true + AudioSource + AudioClip
 *   UI butonu	enableUIButton = true + sahnede bir Button objesi
 *   Görsel geri bildirim	enableVisualFeedback = true + renk tanımı 
 */


public class InteractiveObject : MonoBehaviour
{
    [Header("Etkileşim Türleri")]
    public bool isClickable = false;
    public bool isDraggable = false;
    public bool isTriggerable = false;  //Obje yaklaşınca tetiklenme için kullanılıyor.

    [Header("Seçilebilirlik")]
    public bool enableAnimation = false;
    public bool enableSound = false;
    public bool enableUIButton = false;
    public bool enableVisualFeedback = false;

    [Header("Animasyon")]
    public Animator animator;
    public string triggerAnimationName = "Interact";

    [Header("Ses Efekti")]
    public AudioClip interactionSound;
    private AudioSource audioSource;

    [Header("UI Butonu")]
    public GameObject interactionButton;

    [Header("Görsel Geri Bildirim")]
    public bool disableAfterInteraction = false;
    public Color interactedColor = Color.gray;

    private Vector3 offset;
    private bool isDragging = false;
    private bool hasInteracted = false;

    [Header("Karakter Ruh Hali Değişimi")]
    public EnumCharacterMood moodOnInteract = EnumCharacterMood.Happy;


    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        if (interactionButton != null)
            interactionButton.SetActive(false);
    }

    void OnMouseDown()
    {
        if (hasInteracted) return;

        //Tıklanabilir bir obje ise aşağıdaki işlemler yapılıyor.
        if (isClickable)
        {
            //TriggerInteraction();
        }            

        //Sürüklenebilir bir obje ise aşağıdaki işlemler yapılıyor.
        if (isDraggable)
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            offset = transform.position - new Vector3(mouseWorldPos.x, mouseWorldPos.y, transform.position.z);
            isDragging = true;
        }

        // Ses başlat   --- Mouse tıklanıldığında tetikleniyor.
        if (enableSound && interactionSound != null && audioSource != null)
        {
            audioSource.clip = interactionSound;
            audioSource.loop = true;
            audioSource.Play();
        }

    }

    void OnMouseUp()
    {
        isDragging = false;

        // Ses durdur
        if (enableSound && audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    void Update()
    {
        if (isDragging)
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = new Vector3(mouseWorldPos.x, mouseWorldPos.y, transform.position.z) + offset;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasInteracted) return;

        //Eğer mevcut obje ile yakın olan obje arasında kıyaslama var. Tetiklenme oluyorsa yapılacak işlemler aşağıda yapılıyor
        if (isTriggerable && other.CompareTag("Esya"))
        {
            TriggerInteraction();

            //Karakterin mod değişimi olacaksa bu fonksiyon özelinde oluyor.
            updateCharacterModeState(other);
        }            
    }

    public void TriggerInteraction()
    {
        Debug.Log($"{gameObject.name} ile etkileşim gerçekleşti.");

        if (enableAnimation && animator != null && !string.IsNullOrEmpty(triggerAnimationName))
            animator.SetTrigger(triggerAnimationName);


        if (enableUIButton && interactionButton != null)
            interactionButton.SetActive(true);

        if (enableVisualFeedback && disableAfterInteraction)
        {
            GetComponent<SpriteRenderer>().color = interactedColor;
            hasInteracted = true;
        }



    }

    void updateCharacterModeState(Collider2D other)
    {
        // Karakterin ruh halini değiştir
        CharacterState state = other.GetComponent<CharacterState>();
        if (state != null)
        {
            state.SetMood(moodOnInteract);
            Debug.Log($"Etkileşimi tetikleyen karakter: {other.name}, Ruh hali: {moodOnInteract}");
        }
    }
}
