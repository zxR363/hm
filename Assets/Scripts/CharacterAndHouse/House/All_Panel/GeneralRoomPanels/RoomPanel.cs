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
    public RoomType roomType;
    public Transform objectContainer;
    
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
        // Example: "RoomPanel1/Kitchen/Fridge/Apple"
        string path = obj.name.Replace("(Clone)", "").Trim();
        Transform current = obj.transform.parent;
        
        // Traverse up until we hit the RoomPanel's transform or null
        while (current != null && current != this.transform)
        {
            path = $"{current.name.Replace("(Clone)", "").Trim()}/{path}";
            current = current.parent;
        }
        
        // Prepend RoomPanel name for global uniqueness in the scene file
        return $"{this.name}/{path}";
    }

    private RoomPanel GetOwnerPanel(Transform objTr)
    {
        // Find the CLOSEST RoomPanel in the parent hierarchy
        // This ensures that if we have nested RoomPanels, the child panel owns its objects,
        // and the parent panel ignores them.
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
            
            Debug.Log($"[RoomPanel] Applied saved state to registered object: {uniqueID}");
        }
        else
        {
             // Initial update if no save data
             UpdateObjectData(data, obj);
        }

        trackedObjects.Add(id, data);
        
        Debug.Log($"[RoomPanel] Registered {obj.name} as {uniqueID}");
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
        if (trackedObjects.TryGetValue(id, out RoomObjectData data))
        {
            UpdateObjectData(data, obj);
        }
        else
        {
            // Should be registered, but if not, register it now
            RegisterObject(obj);
        }

        if (saveNow)
        {
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
        }
    }

    public void SaveRoomState()
    {
        string filename = SceneManager.GetActiveScene().name + ".json";
        RoomDataWrapper wrapper;

        // 1. Load existing data
        if (PersistenceManager.Exists(filename))
        {
            try 
            {
                wrapper = PersistenceManager.Load<RoomDataWrapper>(filename);
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

        // 2. Remove OLD data belonging to THIS RoomPanel
        // We identify our data by the prefix "RoomPanelName/"
        string myPrefix = this.name + "/";
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
            // Only save this object if THIS RoomPanel is its closest owner.
            // If it belongs to a nested RoomPanel, let that panel save it.
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
            newObjects.Add(data);
            debugSavedPaths.Add(uniqueID);
        }

        // 5. Add new objects to the wrapper
        wrapper.objects.AddRange(newObjects);
        
        PersistenceManager.Save(filename, wrapper);
        
        Debug.Log($"[RoomPanel] Saved {newObjects.Count} objects to {filename}. \nPaths:\n{string.Join("\n", debugSavedPaths)}");
    }

    public void LoadRoomState()
    {
        // Use Scene Name for filename
        string filename = SceneManager.GetActiveScene().name + ".json";
        
        if (!PersistenceManager.Exists(filename)) 
        {
            _savedStateCache = new Dictionary<string, Queue<RoomObjectData>>();
            return;
        }

        RoomDataWrapper wrapper = PersistenceManager.Load<RoomDataWrapper>(filename);
        if (wrapper == null || wrapper.objects == null) 
        {
            _savedStateCache = new Dictionary<string, Queue<RoomObjectData>>();
            return;
        }

        Debug.Log($"[RoomPanel] Loading {wrapper.objects.Count} objects from {filename}");

        // 1. Populate Cache
        _savedStateCache = new Dictionary<string, Queue<RoomObjectData>>();
        foreach (var data in wrapper.objects)
        {
            if (!_savedStateCache.ContainsKey(data.objectID))
                _savedStateCache[data.objectID] = new Queue<RoomObjectData>();
            
            _savedStateCache[data.objectID].Enqueue(data);
        }
        
        // 2. Proactively apply to ALL existing objects in hierarchy (Recursive)
        // This ensures inactive objects or deep hierarchy objects are updated immediately
        // and maintains the same order as SaveRoomState (Depth-First)
        Transform root = transform;
        RoomObject[] allRoomObjects = root.GetComponentsInChildren<RoomObject>(true);

        foreach (RoomObject roomObj in allRoomObjects)
        {
            // OWNERSHIP CHECK for Loading too
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
        
        Debug.Log($"[RoomPanel] Proactively restored {trackedObjects.Count} objects in hierarchy.");
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
                    interaction.RestoreState(isInteracted);
                }
            }
        }
        
        Debug.Log($"[RoomPanel] Restored state for {obj.name}");
    }

    public List<RoomObjectData> GetSavedData()
    {
        string filename = SceneManager.GetActiveScene().name + ".json";
        if (!PersistenceManager.Exists(filename)) return new List<RoomObjectData>();
        
        RoomDataWrapper wrapper = PersistenceManager.Load<RoomDataWrapper>(filename);
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