using UnityEngine;
using System.Collections.Generic;
using System.Linq;

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

    private void Start()
    {
        LoadRoomState();
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

    public void RegisterObject(GameObject obj)
    {
        int id = obj.GetInstanceID();
        if (trackedObjects.ContainsKey(id)) return;

        RoomObjectData data = new RoomObjectData
        {
            objectID = obj.name.Replace("(Clone)", "").Trim(), // Clean up name
            instance = obj,
            position = obj.transform.position,
            rotation = obj.transform.rotation
        };

        UpdateObjectData(data, obj);
        trackedObjects.Add(id, data);
        
        Debug.Log($"[RoomPanel] Registered {obj.name}");
    }

    public void UnregisterObject(GameObject obj)
    {
        int id = obj.GetInstanceID();
        if (trackedObjects.ContainsKey(id))
        {
            trackedObjects.Remove(id);
        }
    }

    public void NotifyObjectChanged(GameObject obj)
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

        // Generic state check (if you have other components like ObjectState)
        // Example:
        // if (obj.TryGetComponent<ObjectState>(out var state))
        // {
        //     data.customStates["isOpen"] = state.IsOpen.ToString();
        // }
    }

    public void SaveRoomState()
    {
        RoomDataWrapper wrapper = new RoomDataWrapper();
        wrapper.objects = trackedObjects.Values.ToList();
        
        string filename = $"Room_{roomType}.json";
        PersistenceManager.Save(filename, wrapper);
    }

    public void LoadRoomState()
    {
        string filename = $"Room_{roomType}.json";
        if (!PersistenceManager.Exists(filename)) return;

        RoomDataWrapper wrapper = PersistenceManager.Load<RoomDataWrapper>(filename);
        if (wrapper == null || wrapper.objects == null) return;

        Debug.Log($"[RoomPanel] Loading {wrapper.objects.Count} objects for {roomType}");

        // Create a lookup for saved data
        Dictionary<string, Queue<RoomObjectData>> savedDataLookup = new Dictionary<string, Queue<RoomObjectData>>();
        foreach (var data in wrapper.objects)
        {
            if (!savedDataLookup.ContainsKey(data.objectID))
                savedDataLookup[data.objectID] = new Queue<RoomObjectData>();
            
            savedDataLookup[data.objectID].Enqueue(data);
        }

        // Iterate over existing children in objectContainer
        if (objectContainer != null)
        {
            foreach (Transform child in objectContainer)
            {
                string cleanName = child.name.Replace("(Clone)", "").Trim();
                
                if (savedDataLookup.TryGetValue(cleanName, out var queue) && queue.Count > 0)
                {
                    RoomObjectData data = queue.Dequeue();
                    
                    // Apply data
                    ApplyDataToObj(child.gameObject, data);
                    
                    // Update tracking
                    int id = child.gameObject.GetInstanceID();
                    data.instance = child.gameObject;
                    if (trackedObjects.ContainsKey(id))
                        trackedObjects[id] = data;
                    else
                        trackedObjects.Add(id, data);
                }
            }
        }
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
        
        Debug.Log($"[RoomPanel] Restored state for {obj.name}");
    }

    public List<RoomObjectData> GetSavedData()
    {
        string filename = $"Room_{roomType}.json";
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