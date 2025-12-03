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


    private void Start()
    {
        initSpawnItem();
    }

    public void ToggleContainer()
    {
        containerAnimator = imageFridge.GetComponent<Animator>();
        if (containerAnimator != null)
        {
            //Debug.Log("ANİMATOR TRIGGER");
            isOpen = !isOpen;
            containerAnimator.SetBool(animatorBoolName, isOpen);
        }
            
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
