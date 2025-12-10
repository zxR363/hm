using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class AutoLoadTabContents : MonoBehaviour
{
    public void LoadAllTabs()
    {
        if (ItemSelectionPanelController.Instance == null)
        {
            Debug.LogError("[AutoLoadTabContents] ItemSelectionPanelController instance not found!");
            return;
        }
        string sceneName = SceneManager.GetActiveScene().name;
        List<GameObject> tabs = ItemSelectionPanelController.Instance.TabContents;

        Debug.Log($"[AutoLoadTabContents] Starting load for Scene: {sceneName}. Found {tabs.Count} tabs.");

        foreach (GameObject tabObj in tabs)
        {
            if (tabObj == null) continue;

            ScrollRect scrollRect = tabObj.GetComponent<ScrollRect>();
            if (scrollRect == null || scrollRect.content == null)
            {
                Debug.LogWarning($"[AutoLoadTabContents] Tab {tabObj.name} does not have a ScrollRect or Content assigned.");
                continue;
            }

            Transform contentTransform = scrollRect.content;
            string folderName = contentTransform.name; 

            // Fallback: If Content is named "Content" (default), use the Tab Object's name
            if (folderName.Equals("Content"))
            {
                folderName = tabObj.name;
                Debug.Log($"[AutoLoadTabContents] Content name is 'Content', falling back to Tab Name: {folderName}");
            }

            // Path: Prefabs/RoomItems/[SceneName]/[ContentName]
            string resourcePath = $"Prefabs/RoomItems/{sceneName}/{folderName}";
            
            Debug.Log($"[AutoLoadTabContents] Loading from: {resourcePath}");

            GameObject[] loadedPrefabs = Resources.LoadAll<GameObject>(resourcePath);

            if (loadedPrefabs.Length > 0)
            {
                Debug.Log($"[AutoLoadTabContents] Found {loadedPrefabs.Length} items for {folderName}. Instantiating...");
                
                foreach (GameObject prefab in loadedPrefabs)
                {
                    GameObject instance = Instantiate(prefab, contentTransform);
                    
                    // PERSISTENCE: Inject the path into the ItemDragPanel
                    ItemDragPanel idp = instance.GetComponent<ItemDragPanel>();
                    if (idp != null)
                    {
                        idp.ResourcePath = resourcePath; 
                    }
                    
                    // Ensure it has necessary components (optional, but good for safety)
                    if (instance.GetComponent<ItemSelection>() == null)
                    {
                        // If your logic requires ItemSelection on these items, add it or ensure prefab has it
                        // instance.AddComponent<ItemSelection>(); 
                    }
                }
            }
            else
            {
                Debug.Log($"[AutoLoadTabContents] No items found at {resourcePath}");
            }
        }

        // Refresh UI after loading all items
        ItemSelectionPanelController.Instance.ForceUpdateVisibility();
    }
}
