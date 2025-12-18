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
    private const string SAVE_FILENAME = "RoomObjectData.json";
    
    // Dictionary for fast lookup: InstanceID -> RoomObjectData
    private Dictionary<int, RoomObjectData> trackedObjects = new Dictionary<int, RoomObjectData>();
    
    // Cache for saved data to apply when objects register
    private Dictionary<string, Queue<RoomObjectData>> _savedStateCache;

    private bool _isLoading = false;

    private void Awake()
    {
        // PHASE 1: LOAD (Prepare Cache)
        LoadRoomStateIntoCache();
    }

    private void Start()
    {
        // PHASE 3: SPAWN MISSING (Late)
        // We wait for all existing scene objects to Register (in their Start).
        // Then we spawn whatever is left in the Cache.
        StartCoroutine(SpawnMissingObjectsRoutine());
    }

    private void OnDisable()
    {
        SaveRoomState();
    }

    private System.Collections.IEnumerator SpawnMissingObjectsRoutine()
    {
        yield return new WaitForEndOfFrame();
        
        Debug.Log($"[RoomPanel] SpawnRoutine Started. Cache Count: {(_savedStateCache != null ? _savedStateCache.Count : 0)}");
        
        if (_savedStateCache != null && _savedStateCache.Count > 0)
        {
            // Snapshot keys to avoid modification issues
            var keys = _savedStateCache.Keys.ToList();
            
            foreach (var key in keys)
            {
                 if (!_savedStateCache.ContainsKey(key)) continue;

                 Queue<RoomObjectData> queue = _savedStateCache[key];
                 if (queue == null || queue.Count == 0) continue;

                 Debug.Log($"[RoomPanel] Spawning Queue for {key}: {queue.Count} items.");
                 
                 // SNAPSHOT: Convert queue to list and clear the original queue
                 // This is CRITICAL because SpawnObjectFromData puts the item BACK into the queue
                 // for RegisterObject to find. If we loop on the live queue, we create an infinite loop.
                 List<RoomObjectData> itemsToSpawn = queue.ToList();
                 queue.Clear();

                 foreach(var data in itemsToSpawn)
                 {
                     Debug.Log($"[RoomPanel] Force Spawning: {data.objectID} from {data.resourcePath}");
                     SpawnObjectFromData(data);
                 }
            }
        }
        
        _isLoading = false;
        _savedStateCache = null; // Cleanup
    }
    
    // ... SaveRoomState remains same (omitted for brevity in replacement, but ensures it matches existing) ...
    // Note: The instruction asked to rewrite SaveRoomState too? No, just "Rewrite RoomPanel".
    // I will preserve SaveRoomState as is, but Replace LoadRoomState logic.

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
        // Debug.Log($"[RoomPanel] Registering: {uniqueID} (IsLoading: {_isLoading})");

        // Check path early (Debug)
        RoomObject ro = obj.GetComponent<RoomObject>();
        string rPath = ro != null ? ro.loadedFromResourcePath : "NULL_COMP";
        Debug.Log($"[RoomPanel] Registering {obj.name}. ID: {uniqueID}. ResourcePath: '{rPath}'");

        // ZOMBIE CHECK (Deletion Persistence)
        if (_isLoading)
        {
            if (_savedStateCache == null)
            {
                 Debug.LogWarning($"[RoomPanel] Registering {uniqueID} but Cache is NULL!");
            }
            else if (!_savedStateCache.ContainsKey(uniqueID))
            {
                 Debug.LogWarning($"[RoomPanel] Registering '{uniqueID}' (Len:{uniqueID.Length}) - NOT FOUND IN CACHE. \nAvailable Keys:\n{string.Join("\n", _savedStateCache.Keys)}");
            }
            else if (_savedStateCache[uniqueID].Count == 0)
            {
                 Debug.LogError($"[RoomPanel] KILLING ZOMBIE: {obj.name} (ID: {uniqueID}) - Cache Entry Empty.");
                 Destroy(obj);
                 return;
            }
        }

        RoomObjectData data = new RoomObjectData
        {
            objectID = uniqueID,
            instance = obj,
            position = obj.transform.localPosition,
            rotation = obj.transform.rotation
        };

        if (_savedStateCache != null && _savedStateCache.TryGetValue(uniqueID, out var queue) && queue.Count > 0)
        {
            Debug.Log($"[RoomPanel] CACHE HIT for {uniqueID}. Applying Data...");
            RoomObjectData savedData = queue.Dequeue();
            ApplyDataToObj(obj, savedData);
            
            data = savedData;
            data.instance = obj;
        }
        else
        {
             Debug.Log($"[RoomPanel {this.GetInstanceID()}] CACHE MISS for {uniqueID}. IsLoading: {_isLoading}. CacheNull: {_savedStateCache == null}. HasKey: {(_savedStateCache != null && _savedStateCache.ContainsKey(uniqueID))}");
             UpdateObjectData(data, obj);
        }

        trackedObjects.Add(id, data);
    }

    private bool _isQuitting = false;

    private void OnApplicationQuit()
    {
        _isQuitting = true;
        SaveRoomState();
    }

    public void UnregisterObject(GameObject obj)
    {
        if (_isQuitting) return;

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
            RegisterObject(obj);
        }

        if (saveNow)
        {
            SaveRoomState();
        }
    }

    private void UpdateObjectData(RoomObjectData data, GameObject obj)
    {
        // CHANGE: Use LocalPosition by default, but match Inspector (Anchored) for UI
        if (obj.TryGetComponent<RectTransform>(out var rect))
        {
            data.position = rect.anchoredPosition3D; 
            data.customStates["anchoredX"] = rect.anchoredPosition.x.ToString();
            data.customStates["anchoredY"] = rect.anchoredPosition.y.ToString();
            
            // Save Anchors and Pivot to ensure position means the same thing on load
            data.customStates["anchorMinX"] = rect.anchorMin.x.ToString();
            data.customStates["anchorMinY"] = rect.anchorMin.y.ToString();
            data.customStates["anchorMaxX"] = rect.anchorMax.x.ToString();
            data.customStates["anchorMaxY"] = rect.anchorMax.y.ToString();
            data.customStates["pivotX"] = rect.pivot.x.ToString();
            data.customStates["pivotY"] = rect.pivot.y.ToString();

            // NEW: Size (Width/Height)
            data.customStates["sizeDeltaX"] = rect.sizeDelta.x.ToString();
            data.customStates["sizeDeltaY"] = rect.sizeDelta.y.ToString();
        }
        else
        {
            data.position = obj.transform.localPosition;
        }
        data.rotation = obj.transform.localRotation;
        
        // NEW: Local Scale (for both UI and 3D)
        data.customStates["scaleX"] = obj.transform.localScale.x.ToString();
        data.customStates["scaleY"] = obj.transform.localScale.y.ToString();
        data.customStates["scaleZ"] = obj.transform.localScale.z.ToString();

        if (obj.TryGetComponent<RoomObjectInteraction>(out var interaction))
        {
            data.customStates["isInteracted"] = interaction.IsInteracted.ToString();
            if(!string.IsNullOrEmpty(interaction.CurrentSourceItemName))
            {
                 data.customStates["interactedSource"] = interaction.CurrentSourceItemName;
            }
        }
    }

    public void SaveRoomState()
    {
        // 1. Load existing data from GLOBAL file
        RoomDataWrapper wrapper = null;
        if (PersistenceManager.Exists(SAVE_FILENAME))
        {
            try { wrapper = PersistenceManager.Load<RoomDataWrapper>(SAVE_FILENAME); }
            catch { wrapper = new RoomDataWrapper(); }
        }
        else { wrapper = new RoomDataWrapper(); }

        if (wrapper == null) wrapper = new RoomDataWrapper();
        if (wrapper.objects == null) wrapper.objects = new List<RoomObjectData>();

        // 2. Remove OLD data belonging to THIS RoomPanel in THIS Scene
        string sceneName = SceneManager.GetActiveScene().name;
        string myPrefix = $"{sceneName}/{this.name}/";
        wrapper.objects.RemoveAll(x => x.objectID.StartsWith(myPrefix));

        // 3. Save ONLY Registered Objects
        List<RoomObjectData> newObjects = new List<RoomObjectData>();
        
        foreach (var kvp in trackedObjects) 
        {
            GameObject obj = kvp.Value.instance;
            RoomObjectData data = kvp.Value;

            if (obj == null) continue; 

            // Update Resource Path
            RoomObject roomObj = obj.GetComponent<RoomObject>();
            if (roomObj != null) 
            {
                 data.resourcePath = roomObj.loadedFromResourcePath;
            }
            
            // DEBUG: Unconditional Log
            // Debug.Log($"[RoomPanel] SAVING LOOP: {obj.name} | Path: '{data.resourcePath}' | ID: {data.objectID}");
            
            // DEBUG: Check if we are saving new objects
            if (!string.IsNullOrEmpty(data.resourcePath))
            {
                Debug.Log($"[RoomPanel] SAVE ITEM: {obj.name} (ID: {data.objectID}) | Path: {data.resourcePath}");
            }
            else
            {
                Debug.LogWarning($"[RoomPanel] SAVING ITEM WITHOUT PATH: {obj.name} (ID: {data.objectID}). This object will NOT respawn if deleted/missing.");
            }

            UpdateObjectData(data, obj);
            data.OnBeforeSerialize();
            newObjects.Add(data);
        }

        wrapper.objects.AddRange(newObjects);
        PersistenceManager.Save(SAVE_FILENAME, wrapper);
    }
    
    // Renamed from LoadRoomState to be internal helper
    private void LoadRoomStateIntoCache()
    {
        if (!PersistenceManager.Exists(SAVE_FILENAME)) return;

        RoomDataWrapper wrapper = PersistenceManager.Load<RoomDataWrapper>(SAVE_FILENAME);
        if (wrapper == null || wrapper.objects == null) return;

        string sceneName = SceneManager.GetActiveScene().name;
        string myPrefix = $"{sceneName}/{this.name}/";
        Debug.Log($"[RoomPanel] LoadRoomStateIntoCache Started. Panel: {this.name}. Prefix: '{myPrefix}'. Objects in JSON: {wrapper.objects.Count}");
        
        _savedStateCache = new Dictionary<string, Queue<RoomObjectData>>();
        
        foreach (var data in wrapper.objects)
        {
            if (data.objectID.StartsWith(myPrefix))
            {
                if (!_savedStateCache.ContainsKey(data.objectID))
                    _savedStateCache[data.objectID] = new Queue<RoomObjectData>();
                
                _savedStateCache[data.objectID].Enqueue(data);
            }
        }
        
        // Flag that we have valid load data
        _isLoading = true;
    }

    // Helper for Spawning
    private void SpawnObjectFromData(RoomObjectData data)
    {
        data.OnAfterDeserialize();
        
        if (!string.IsNullOrEmpty(data.resourcePath))
        {
            GameObject resource = Resources.Load<GameObject>(data.resourcePath);
            if (resource != null)
            {
                // INDIRECTION FIX: If we loaded a Menu Item (ItemDragPanel), get the Real Prefab
                GameObject prefabToSpawn = resource;
                ItemDragPanel idp = resource.GetComponent<ItemDragPanel>();
                if (idp != null && idp.ItemPrefab != null)
                {
                     prefabToSpawn = idp.ItemPrefab;
                }

                Transform parent = objectContainer != null ? objectContainer : transform;

                // PATH RECONSTRUCTION (Restored):
                // The objectID contains the full path (e.g. HouseScene/Content/Room1Panel/Sofa).
                // We need to place it in Room1Panel, not just 'parent' (Content).
                 string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                 // Assuming 'this.name' is 'Content' or the Manager's name. 
                 // If this script is on 'Content', myPrefix is "HouseScene/Content/".
                 
                 // Dynamic Prefix Calculation
                 // We want to find the path relative to 'parent'.
                 // If parent is 'Content', and ID is '.../Content/Room1Panel/Sofa', we want 'Room1Panel'.
                 
                 // Strategy: Find the child in 'parent' that matches the path segment in ID.
                 // 1. Get Path relative to 'parent'? 
                 // Hard to know parent's full path string if it's not generated same way.
                 // But we know ID is "Scene/Parent/.../Item".
                 
                 // Robust Approach:
                 // Check if any child of 'parent' is mentioned in the ID.
                 foreach (Transform child in parent)
                 {
                     if (data.objectID.Contains("/" + child.name + "/"))
                     {
                         parent = child;
                         // Support nested? Only 1 level deep for now as per user structure (RoomPanel).
                         // Could be recursive but let's stick to RoomPanel level.
                         break;
                     }
                 }

                // CRITICAL FIX: Put data in Cache BEFORE Instantiate so RegisterObject finds it immediately.
                if (!_savedStateCache.ContainsKey(data.objectID))
                    _savedStateCache[data.objectID] = new Queue<RoomObjectData>();
                _savedStateCache[data.objectID].Enqueue(data);
                
                Debug.Log($"[RoomPanel {this.GetInstanceID()}] Pre-Spawn Cache Injection for {data.objectID}. Queue Size: {_savedStateCache[data.objectID].Count}");

                GameObject instance = Instantiate(prefabToSpawn, parent);
                
                // ENSURE ROOM OBJECT EXISTS & HAS PATH
                RoomObject roomObj = instance.GetComponent<RoomObject>();
                if (roomObj == null) roomObj = instance.AddComponent<RoomObject>();
                roomObj.loadedFromResourcePath = data.resourcePath;
                
                string[] parts = data.objectID.Split('/');
                if (parts.Length > 0) instance.name = parts[parts.Length - 1]; 
                
                // CRITICAL FIX: Manually register immediately while Cache is valid.
                Debug.Log($"[RoomPanel] Manual Register: {instance.name}");
                RegisterObject(instance);
                // So we should NOT Dequeue?
                // No, we already Dequeued.
                
                // FIX: Temporarily put it back in cache or handle registration explicitly?
                // Simpler: RegisterObject is called in Start().
                // We can just ApplyDataToObj here, and when RegisterObject is called, it sees it's already updated?
                // No. RegisterObject might reset or look for data.
                
            }
                
                // Instantiate triggers Awake/Start immediately?
                // Yes. So RegisterObject runs recursively HERE.
                // So putting it back in cache works perfectly.
            }
        }


    public void LoadRoomState()
    {
        if (!PersistenceManager.Exists(SAVE_FILENAME)) return;

        RoomDataWrapper wrapper = PersistenceManager.Load<RoomDataWrapper>(SAVE_FILENAME);
        if (wrapper == null || wrapper.objects == null) return;

        // Filter data for THIS RoomPanel
        string sceneName = SceneManager.GetActiveScene().name;
        string myPrefix = $"{sceneName}/{this.name}/";
        Debug.Log($"[RoomPanel] LoadRoomState Started. MyPrefix: '{myPrefix}'. Total Objects in JSON: {wrapper.objects.Count}");

        List<RoomObjectData> myData = new List<RoomObjectData>();
        foreach(var obj in wrapper.objects)
        {
             if (obj.objectID.StartsWith(myPrefix))
             {
                 myData.Add(obj);
                 //Debug.Log($"[RoomPanel] Accepted ID: {obj.objectID}");
             }
             else
             {
                 //Debug.Log($"[RoomPanel] Rejected ID: {obj.objectID} (No Match)");
             }
        }

        // Map for fast lookup of SAVED data
        Dictionary<string, RoomObjectData> savedMap = myData.ToDictionary(x => x.objectID, x => x);

        // --- STEP 1: PRUNING (Delete objects not in Save File) ---
        // We look at currently tracked objects. If their ID is NOT in the savedMap, they are deleted.
        List<int> toDestroyKeys = new List<int>();
        
        foreach (var kvp in trackedObjects)
        {
            // If instance is null, just remove from dictionary later
            if (kvp.Value.instance == null) 
            {
                toDestroyKeys.Add(kvp.Key);
                continue;
            }

            string currentID = kvp.Value.objectID;
            
            // Check existence in Save Data
            if (!savedMap.ContainsKey(currentID))
            {
                // This object exists in scene but was NOT in the save file. DELETE IT.
                // Assuming "Save File Exists" means we have a complete state.
                // NOTE: Static objects should be in the save file if they were saved at least once.
                // If this is the FIRST run ever, specific logic might be needed, but usually Save happens on Disable.
                Debug.Log($"[RoomPanel] Deleting missing object: {kvp.Value.instance.name} ({currentID})");
                Destroy(kvp.Value.instance);
                toDestroyKeys.Add(kvp.Key);
            }
            else
            {
                // It exists. Update it.
                RoomObjectData savedData = savedMap[currentID];
                savedData.OnAfterDeserialize(); // Restore Dictionary
                ApplyDataToObj(kvp.Value.instance, savedData);
                
                // Update track record
                trackedObjects[kvp.Key] = savedData;
                savedData.instance = kvp.Value.instance;
                
                // Remove from map to mark as "Handled"
                savedMap.Remove(currentID);
            }
        }

        // Clean up dictionary
        foreach (int key in toDestroyKeys) trackedObjects.Remove(key);


        // --- STEP 2: SPAWNING (Create objects in Save File but not in Scene) ---
        // Any item remaining in savedMap is "Missing" and needs to be spawned.
        
        // Cache data for ApplyData (RoomObject.Register will call this)
        if (_savedStateCache == null) _savedStateCache = new Dictionary<string, Queue<RoomObjectData>>();
        
        foreach (var kvp in savedMap)
        {
            RoomObjectData data = kvp.Value;
            data.OnAfterDeserialize();

            // Store in cache so when it registers, it gets this data
            if (!_savedStateCache.ContainsKey(data.objectID))
                _savedStateCache[data.objectID] = new Queue<RoomObjectData>();
            _savedStateCache[data.objectID].Enqueue(data);

            // Instantiate if possible
            if (!string.IsNullOrEmpty(data.resourcePath))
            {
                GameObject prefab = Resources.Load<GameObject>(data.resourcePath);
                if (prefab != null)
                {
                    Transform parent = objectContainer != null ? objectContainer : transform;
                    
                    // CRITICAL FIX: Put data in Cache BEFORE Instantiate so RegisterObject finds it immediately.
                    if (!_savedStateCache.ContainsKey(data.objectID))
                        _savedStateCache[data.objectID] = new Queue<RoomObjectData>();
                    _savedStateCache[data.objectID].Enqueue(data);

                    Debug.Log($"[RoomPanel {this.GetInstanceID()}] Pre-Spawn Cache Injection for {data.objectID}. Queue Size: {_savedStateCache[data.objectID].Count} | IsLoading: {_isLoading}");

                    GameObject instance = Instantiate(prefab, parent);
                    
                    // Set Name to match ID suffix for consistency
                    // Extract name from ID: "Scene/Panel/ObjName"
                    string[] parts = data.objectID.Split('/');
                    if (parts.Length > 0) instance.name = parts[parts.Length - 1]; 
                    
                    // CRITICAL FIX: Manually register immediately while Cache is valid.
                    // RoomObject.Start() runs too late (after cache cleanup).
                    RegisterObject(instance);
                    
                    Debug.Log($"[RoomPanel] Spawned {instance.name} from {data.resourcePath}");
                }
                else
                {
                    Debug.LogWarning($"[RoomPanel] Failed to load resource: {data.resourcePath}");
                }
            }
            else
            {
                // Static object that was deleted? No, if it was static and not in scene, it's gone?
                // Or static object that was mistakenly destroyed?
                // Without resourcePath, we can't spawn it.
                 Debug.LogWarning($"[RoomPanel] Cannot spawn {data.objectID}: No ResourcePath saved.");
            }
        }
        
        // Cleanup cache logic is handled in RegisterObject mostly, but we can clear it next frame?
        // No, keep it for the frame.
    }

    private void ApplyDataToObj(GameObject obj, RoomObjectData data)
    {
        // NEW: Apply Scale (Generic for all objects)
        if (data.customStates.TryGetValue("scaleX", out var scX) && 
            data.customStates.TryGetValue("scaleY", out var scY) && 
            data.customStates.TryGetValue("scaleZ", out var scZ))
        {
            if (float.TryParse(scX, out float sX) && float.TryParse(scY, out float sY) && float.TryParse(scZ, out float sZ))
            {
                obj.transform.localScale = new Vector3(sX, sY, sZ);
            }
        }

        if (obj.TryGetComponent<RectTransform>(out var rect))
        {
            // CHANGE: Restore Anchors/Pivot FIRST to ensure coordinate system matches
            if (data.customStates.TryGetValue("anchorMinX", out var amX) && data.customStates.TryGetValue("anchorMinY", out var amY))
            {
                if (float.TryParse(amX, out float minX) && float.TryParse(amY, out float minY))
                    rect.anchorMin = new Vector2(minX, minY);
            }
            
            if (data.customStates.TryGetValue("anchorMaxX", out var axX) && data.customStates.TryGetValue("anchorMaxY", out var axY))
            {
                if (float.TryParse(axX, out float maxX) && float.TryParse(axY, out float maxY))
                    rect.anchorMax = new Vector2(maxX, maxY);
            }
            
            if (data.customStates.TryGetValue("pivotX", out var pX) && data.customStates.TryGetValue("pivotY", out var pY))
            {
                if (float.TryParse(pX, out float pivX) && float.TryParse(pY, out float pivY))
                    rect.pivot = new Vector2(pivX, pivY);
            }

            // CHANGE: Force update to ensure anchors take effect before position
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(rect);

            // NEW: Apply SizeDelta (Width/Height) if saved
            if (data.customStates.TryGetValue("sizeDeltaX", out var szX) && data.customStates.TryGetValue("sizeDeltaY", out var szY))
            {
                if (float.TryParse(szX, out float sdX) && float.TryParse(szY, out float sdY))
                {
                    rect.sizeDelta = new Vector2(sdX, sdY);
                }
            }
            
            // CHANGE: Direct apply from stored Position (which is now AnchoredPosition3D)
            Debug.Log($"[RoomPanel] Applying Persistence to {obj.name}: Pos3D={data.position} | Anchors=({rect.anchorMin}, {rect.anchorMax}) | Pivot={rect.pivot} | Size={rect.sizeDelta}");
            rect.anchoredPosition3D = data.position;
            
            // Legacy/Fallback check (Optional, but direct assignment is preferred now)
            /*
            if (data.customStates.TryGetValue("anchoredX", out var xStr) &&
                data.customStates.TryGetValue("anchoredY", out var yStr))
            {
                if (float.TryParse(xStr, out float x) && float.TryParse(yStr, out float y))
                {
                    rect.anchoredPosition = new Vector2(x, y);
                }
            }
            */
        }
        else
        {
            obj.transform.localPosition = data.position;
            obj.transform.localRotation = data.rotation;
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