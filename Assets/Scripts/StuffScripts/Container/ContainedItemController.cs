using UnityEngine;

public class ContainedItemController : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    private Vector3 originalScale;

    public bool isAttachedToContainer = true;
    public ContainerController containerController;

    private bool hasInitialized = false;


    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }           

        originalScale = transform.localScale;
        //HideCompletely();
    }

    void Update()
    {
        if (!hasInitialized || containerController == null || containerController.containerAreaCollider == null)
            return;

        // Dolap alanı içinde mi?
        bool isInside = IsInsideContainerArea();

        if (isAttachedToContainer && !isInside)
        {
            DetachFromContainer();
        }
        else if (!isAttachedToContainer && isInside)
        {
            ReattachToContainer();
        }

    }

    private bool IsInsideContainerArea()
    {
        bool result = containerController != null &&
               containerController.containerAreaCollider != null &&
               containerController.containerAreaCollider.bounds.Contains(transform.position);
        return result;
    }



    private void DetachFromContainer()
    {
        isAttachedToContainer = false;
        transform.SetParent(null);
        
        // Artık dolapla ilişki yok
        if (containerController != null)
        {
            containerController.spawnedItems.Remove(this); // Listeden çıkar
        }

        //Debug.Log($"{gameObject.name} dolaptan ayrıldı.");
    }

    private void ReattachToContainer()
    {
        isAttachedToContainer = true;
        transform.SetParent(containerController.spawnParent);
        transform.localPosition = Vector3.zero;

        // Listeye tekrar ekle (daha önce çıkarılmış olabilir)
        if (!containerController.spawnedItems.Contains(this))
        {
            containerController.spawnedItems.Add(this);
        }

        //Debug.Log($"{gameObject.name} dolaba geri döndü.");
    }


    public void ShowFully()
    {       
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                Debug.LogError($"CanvasGroup missing on {gameObject.name}");
                return;
            }
        }
        gameObject.SetActive(true);
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        transform.localScale = originalScale;
        hasInitialized = true;
    }

    public void HideCompletely()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        transform.localScale = originalScale * 0.8f;
        gameObject.SetActive(false);
    }
}
