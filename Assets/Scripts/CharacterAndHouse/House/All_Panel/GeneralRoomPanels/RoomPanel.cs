using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

[System.Serializable]
public class RoomDataWrapper
{
    public List<RoomObjectData> objects = new List<RoomObjectData>();
}

public class RoomPanel : MonoBehaviour
{
    public RoomType roomType; //Silinecek
    public Transform objectContainer; //Silinecek
    
    // Constant filename for the entire game
    private const string SAVE_FILENAME = "AvatarWorldData.json";
    
    // Dictionary for fast lookup: InstanceID -> RoomObjectData
    private Dictionary<int, RoomObjectData> trackedObjects = new Dictionary<int, RoomObjectData>();
    
    // Cache for saved data to apply when objects register
    private Dictionary<string, Queue<RoomObjectData>> _savedStateCache;

    private void Awake()
    {
        // Load data early so it's ready for Start/Register
        LoadRoomState();
    }

    private void Start()
    {
        // LoadRoomState is now in Awake
    }

    private void OnDisable()
    {
        SaveRoomState();
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause) SaveRoomState();
    }

    private void OnApplicationQuit()
    {
        SaveRoomState();
    }

    private string GetUniqueID(GameObject obj)
    {
        // Build hierarchy path relative to this RoomPanel
        string path = obj.name.Replace("(Clone)", "").Trim();
        Transform current = obj.transform.parent;
        
        while (current != null && current != this.transform)
        {
            path = $"{current.name.Replace("(Clone)", "").Trim()}/{path}";
            current = current.parent;
        }
        
        // Include SCENE NAME and RoomPanel Name for global uniqueness
        // Example: "HouseScene/RoomPanel1/Kitchen/Fridge/Apple"
        string sceneName = SceneManager.GetActiveScene().name;
        return $"{sceneName}/{this.name}/{path}";
    }

    private RoomPanel GetOwnerPanel(Transform objTr)
    {
        // Find the CLOSEST RoomPanel in the parent hierarchy
        Transform curr = objTr;
        while (curr != null)
        {
            if (curr.TryGetComponent<RoomPanel>(out var panel))
            {
                return panel;
            }
            curr = curr.parent;
        }
        return null;
    }

    public void RegisterObject(GameObject obj)
    {
        int id = obj.GetInstanceID();
        if (trackedObjects.ContainsKey(id)) return;

        string uniqueID = GetUniqueID(obj);

        RoomObjectData data = new RoomObjectData
        {
            objectID = uniqueID,
            instance = obj,
            position = obj.transform.position,
            rotation = obj.transform.rotation
        };

        // If we have saved data for this object, apply it NOW
        if (_savedStateCache != null && _savedStateCache.TryGetValue(uniqueID, out var queue) && queue.Count > 0)
        {
            RoomObjectData savedData = queue.Dequeue();
            ApplyDataToObj(obj, savedData);
            
            // Use the saved data structure but update the instance reference
            data = savedData;
            data.instance = obj;
            
            //Debug.Log($"[RoomPanel] Applied saved state to registered object: {uniqueID}");
        }
        else
        {
             // Initial update if no save data
             UpdateObjectData(data, obj);
        }

        trackedObjects.Add(id, data);
        
        //Debug.Log($"[RoomPanel] Registered {obj.name} as {uniqueID}");
    }

    public void UnregisterObject(GameObject obj)
    {
        int id = obj.GetInstanceID();
        if (trackedObjects.ContainsKey(id))
        {
            trackedObjects.Remove(id);
        }
    }

    public void NotifyObjectChanged(GameObject obj, bool saveNow = false)
    {
        int id = obj.GetInstanceID();
        //Debug.Log($"[RoomPanel] NotifyObjectChanged received for {obj.name} (ID: {id}). SaveNow: {saveNow}");
        
        if (trackedObjects.TryGetValue(id, out RoomObjectData data))
        {
            UpdateObjectData(data, obj);
        }
        else
        {
            //Debug.Log($"[RoomPanel] Object {obj.name} not found in tracked list. Registering now.");
            // Should be registered, but if not, register it now
            RegisterObject(obj);
        }

        if (saveNow)
        {
            //Debug.Log("[RoomPanel] Triggering Immediate Save.");
            SaveRoomState();
        }
    }

    private void UpdateObjectData(RoomObjectData data, GameObject obj)
    {
        // UI Object check
        if (obj.TryGetComponent<RectTransform>(out var rect))
        {
            data.customStates["anchoredX"] = rect.anchoredPosition.x.ToString();
            data.customStates["anchoredY"] = rect.anchoredPosition.y.ToString();
        }
        else
        {
            data.position = obj.transform.position;
            data.rotation = obj.transform.rotation;
        }

        // Save Interaction State
        if (obj.TryGetComponent<RoomObjectInteraction>(out var interaction))
        {
            data.customStates["isInteracted"] = interaction.IsInteracted.ToString();
            if(!string.IsNullOrEmpty(interaction.CurrentSourceItemName))
            {
                 data.customStates["interactedSource"] = interaction.CurrentSourceItemName;
                 Debug.Log($"[RoomPanel] Saving Interaction: {obj.name} -> Source: {interaction.CurrentSourceItemName}");
            }
        }
    }

    public void SaveRoomState()
    {
        RoomDataWrapper wrapper;

        // 1. Load existing data from GLOBAL file
        if (PersistenceManager.Exists(SAVE_FILENAME))
        {
            try 
            {
                wrapper = PersistenceManager.Load<RoomDataWrapper>(SAVE_FILENAME);
            }
            catch
            {
                wrapper = new RoomDataWrapper();
            }
        }
        else
        {
            wrapper = new RoomDataWrapper();
        }

        if (wrapper == null) wrapper = new RoomDataWrapper();
        if (wrapper.objects == null) wrapper.objects = new List<RoomObjectData>();

        // 2. Remove OLD data belonging to THIS RoomPanel in THIS Scene
        // Prefix: "SceneName/RoomPanelName/"
        string sceneName = SceneManager.GetActiveScene().name;
        string myPrefix = $"{sceneName}/{this.name}/";
        
        wrapper.objects.RemoveAll(x => x.objectID.StartsWith(myPrefix));

        // 3. Get current objects from THIS RoomPanel (Recursive)
        Transform root = transform; 
        RoomObject[] myObjects = root.GetComponentsInChildren<RoomObject>(true);
        
        List<RoomObjectData> newObjects = new List<RoomObjectData>();
        List<string> debugSavedPaths = new List<string>();

        // 4. Create new data for current objects
        foreach (RoomObject roomObj in myObjects)
        {
            // OWNERSHIP CHECK:
            if (GetOwnerPanel(roomObj.transform) != this)
            {
                continue;
            }

            GameObject obj = roomObj.gameObject;
            string uniqueID = GetUniqueID(obj);

            RoomObjectData data = new RoomObjectData
            {
                objectID = uniqueID,
                instance = obj,
                position = obj.transform.position,
                rotation = obj.transform.rotation
            };
            
            UpdateObjectData(data, obj);
            
            // FORCE SERIALIZATION: Ensure dictionary is flushed to lists
            data.OnBeforeSerialize();
            
            newObjects.Add(data);
            debugSavedPaths.Add(uniqueID);
        }

        // 5. Add new objects to the wrapper
        wrapper.objects.AddRange(newObjects);
        
        PersistenceManager.Save(SAVE_FILENAME, wrapper);
        
        //Debug.Log($"[RoomPanel] Saved {newObjects.Count} objects to {SAVE_FILENAME}. \nPaths:\n{string.Join("\n", debugSavedPaths)}");
    }

    public void LoadRoomState()
    {
        if (!PersistenceManager.Exists(SAVE_FILENAME)) 
        {
            _savedStateCache = new Dictionary<string, Queue<RoomObjectData>>();
            return;
        }

        RoomDataWrapper wrapper = PersistenceManager.Load<RoomDataWrapper>(SAVE_FILENAME);
        if (wrapper == null || wrapper.objects == null) 
        {
            _savedStateCache = new Dictionary<string, Queue<RoomObjectData>>();
            return;
        }

        //Debug.Log($"[RoomPanel] Loading objects from {SAVE_FILENAME}");

        // 1. Populate Cache
        _savedStateCache = new Dictionary<string, Queue<RoomObjectData>>();
        foreach (var data in wrapper.objects)
        {
            if (!_savedStateCache.ContainsKey(data.objectID))
                _savedStateCache[data.objectID] = new Queue<RoomObjectData>();
            
            _savedStateCache[data.objectID].Enqueue(data);
        }
        
        // 2. Proactively apply to ALL existing objects in hierarchy (Recursive)
        Transform root = transform;
        RoomObject[] allRoomObjects = root.GetComponentsInChildren<RoomObject>(true);

        foreach (RoomObject roomObj in allRoomObjects)
        {
            // OWNERSHIP CHECK
            if (GetOwnerPanel(roomObj.transform) != this)
            {
                continue;
            }

            string uniqueID = GetUniqueID(roomObj.gameObject);
            
            if (_savedStateCache.TryGetValue(uniqueID, out var queue) && queue.Count > 0)
            {
                RoomObjectData savedData = queue.Dequeue();
                ApplyDataToObj(roomObj.gameObject, savedData);
                
                // Update tracking immediately
                int id = roomObj.gameObject.GetInstanceID();
                
                // Update the data instance reference
                savedData.instance = roomObj.gameObject;
                
                if (trackedObjects.ContainsKey(id))
                    trackedObjects[id] = savedData;
                else
                    trackedObjects.Add(id, savedData);
            }
        }

        // CRITICAL FIX: Clear the cache after initial load.
        // Any data remaining in the cache corresponds to objects that do not exist in the scene.
        // Keeping it causes "Zombie Data" bugs where new items (dragged later) accidentally 
        // match the ID of a deleted/missing item and snap to its old position.
        _savedStateCache.Clear();
        
        //Debug.Log($"[RoomPanel] Proactively restored {trackedObjects.Count} objects in hierarchy.");
    }

    private void ApplyDataToObj(GameObject obj, RoomObjectData data)
    {
        if (obj.TryGetComponent<RectTransform>(out var rect))
        {
            if (data.customStates.TryGetValue("anchoredX", out var xStr) &&
                data.customStates.TryGetValue("anchoredY", out var yStr))
            {
                if (float.TryParse(xStr, out float x) && float.TryParse(yStr, out float y))
                {
                    rect.anchoredPosition = new Vector2(x, y);
                }
            }
        }
        else
        {
            obj.transform.position = data.position;
            obj.transform.rotation = data.rotation;
        }
        
        // Restore Interaction State
        if (obj.TryGetComponent<RoomObjectInteraction>(out var interaction))
        {
            if (data.customStates.TryGetValue("isInteracted", out var interactedStr))
            {
                if (bool.TryParse(interactedStr, out bool isInteracted))
                {
                    // Restore source Name if available
                    string sourceName = null;
                    if (data.customStates.TryGetValue("interactedSource", out var sName))
                    {
                        sourceName = sName;
                        Debug.Log($"[RoomPanel] Loading Interaction: {obj.name} -> Found Source: {sourceName}");
                    }
                    else
                    {
                        // Debug.Log($"[RoomPanel] Loading Interaction: {obj.name} -> No Source Key Found");
                    }
                    
                    interaction.RestoreState(isInteracted, sourceName);
                }
            }
        }
        
        //Debug.Log($"[RoomPanel] Restored state for {obj.name}");
    }

    public List<RoomObjectData> GetSavedData()
    {
        if (!PersistenceManager.Exists(SAVE_FILENAME)) return new List<RoomObjectData>();
        
        RoomDataWrapper wrapper = PersistenceManager.Load<RoomDataWrapper>(SAVE_FILENAME);
        return wrapper != null ? wrapper.objects : new List<RoomObjectData>();
    }
    
    public void ClearObjects()
    {
        foreach (var kvp in trackedObjects)
        {
            if (kvp.Value.instance != null)
                Destroy(kvp.Value.instance);
        }
        trackedObjects.Clear();
    }
}