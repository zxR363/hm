using System.Collections.Generic;
using UnityEngine;

public class ContainerController : MonoBehaviour
{
    [SerializeField] private GameObject imageFridge;
    public Animator containerAnimator;
    public string openStateName = "Open_Container";
    public string closeStateName = "Idle_Container";
    public string animatorBoolName = "IsOpen";

    public List<GameObject> itemPrefabs;
    public Transform spawnParent;
    public List<ContainedItemController> spawnedItems = new List<ContainedItemController>();

    private bool isOpen = false;
    private bool hasShownItems = false;
    private bool objectStartFlag = false;

    public BoxCollider2D containerAreaCollider; // Dolap alanı


    private void Awake()
    {
        // OPTIMIZATION: Ensure Animator is asleep immediately
        // This must run before initSpawnItem to avoid being skipped by errors.
        if (imageFridge != null)
        {
            containerAnimator = imageFridge.GetComponent<Animator>();
            if (containerAnimator != null)
            {
                containerAnimator.enabled = false;
            }
        }
    }

    private void Start()
    {
        // DEBUG: Disabling spawning to isolate "Missing Script" errors and Layout Loop.
        // initSpawnItem();
    }

    public void ToggleContainer()
    {
        if (imageFridge == null) return;
        
        containerAnimator = imageFridge.GetComponent<Animator>();
        if (containerAnimator != null)
        {
            // Enable Animator to play the transition
            containerAnimator.enabled = true;
            
            //Debug.Log("ANİMATOR TRIGGER");
            isOpen = !isOpen;
            containerAnimator.SetBool(animatorBoolName, isOpen);
            
            // Apply optimization: Disable animator after transition finishes
            StopAllCoroutines();
            StartCoroutine(DisableAnimatorAfterTransition(containerAnimator));
        }
    }

    private System.Collections.IEnumerator DisableAnimatorAfterTransition(Animator anim)
    {
        // Wait for the transition to start
        yield return null; 
        
        // Wait while transitioning or playing
        while (anim != null && anim.enabled && IsPlaying(anim))
        {
            yield return new WaitForSeconds(0.1f); // Check every 100ms
        }
        
        // Wait a tiny bit more to ensure frame settles
        yield return null;

        if (anim != null)
        {
            // Keep enabled if it's a loop, but usually for UI panels we want to rest.
            // If the user wants continuous looping, they should use a separate Loop state.
            // For now, assume 'Idle' and 'Open' are static states.
            anim.enabled = false;
        }
    }

    private bool IsPlaying(Animator anim)
    {
        if (anim.IsInTransition(0)) return true;
        
        // Check if current state animation is still playing (normalizedTime < 1)
        // OR if it's a looping animation (loop = true), we can't optimize it this way.
        // Assuming non-looping UI animations for Open/Close.
        var stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.normalizedTime < 1.0f) return true;
        
        return false;
    }

    public void OnContainerOpened()
    {
        if (objectStartFlag == false)
        {
            ShowItems();
            objectStartFlag = true;
        }

        if (hasShownItems == false)
        {
            ShowItems();
            hasShownItems = true;
        }
    }

    public void OnContainerClosing()
    {
        if (hasShownItems == true)
        {
            foreach (var item in spawnedItems)
            {
                item.HideCompletely();
            }
            hasShownItems = false;
        }
    }

    private void ShowItems()
    {
        foreach (var item in spawnedItems)
        {
            item.ShowFully();
        }
    }

    public void SpawnItem(GameObject item, RectTransform spawnPoint)
    {
        RectTransform itemRT = item.GetComponent<RectTransform>();

        if (itemRT != null && spawnPoint != null)
        {
            // Boyutu eşitle
            itemRT.sizeDelta = spawnPoint.sizeDelta;

            // Pozisyon ve hizalama
            itemRT.anchoredPosition = Vector2.zero;
            itemRT.anchorMin = spawnPoint.anchorMin;
            itemRT.anchorMax = spawnPoint.anchorMax;
            itemRT.pivot = spawnPoint.pivot;

            // Ölçek sabitle
            itemRT.localScale = Vector3.one;
        }
        else
        {
            Debug.LogWarning("RectTransform eksik: Spawn işlemi yapılamadı.");
        }
    }


    public void initSpawnItem()
    {
        if (spawnedItems.Count == 0)
        {
            foreach (var prefab in itemPrefabs)
            {

                RectTransform spawnRT = spawnParent.GetComponent<RectTransform>();
                GameObject item = Instantiate(prefab, spawnRT);

                SpawnItem(item, spawnRT);

                // Debug.Log("Item position=" + item.transform.position);
                // Debug.Log("Spawn position=" + spawnParent.transform.position);
                // Debug.Log("Fridge position=" + this.transform.position);

                //-------item, spawnParent objesinin tam merkezine yerleşir.--///

                var controller = item.GetComponent<ContainedItemController>();
                if (controller != null)
                {
                    spawnedItems.Add(controller);
                    controller.containerController = this; // Referansı ver
                    item.SetActive(false);
                }
            }
        }
    }
}
