using System.Collections.Generic;
using UnityEngine;

public class ContainerController : MonoBehaviour
{
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

    public void ToggleContainer()
    {
        isOpen = !isOpen;
        containerAnimator.SetBool(animatorBoolName, isOpen);
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
        if (spawnedItems.Count == 0)
        {
            foreach (var prefab in itemPrefabs)
            {
                GameObject item = Instantiate(prefab, spawnParent);

                //-------item, spawnParent objesinin tam merkezine yerleşir.--///

                item.transform.SetParent(spawnParent, false); // UI için local pozisyonu korur

                // UI objesi ise:
                RectTransform rt = item.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchoredPosition = Vector2.zero;
                }
                else
                {
                    item.transform.localPosition = Vector3.zero;
                }
                //-------item, spawnParent objesinin tam merkezine yerleşir.--///

                var controller = item.GetComponent<ContainedItemController>();
                if (controller != null)
                {
                    spawnedItems.Add(controller);
                    controller.ShowFully();
                }
            }
        }
        else
        {
            foreach (var item in spawnedItems)
            {
                item.ShowFully();
            }
        }
    }
}
