using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

[System.Serializable]
public class RoomDataWrapper
{
    public List<RoomObjectData> objects = new List<RoomObjectData>();
    public List<string> deletedObjectIDs = new List<string>(); // Support for explicit deletion persistence
}

public class RoomPanel : MonoBehaviour
{
    public RoomType roomType; // Legacy field
    public Transform objectContainer; // Legacy field
    
    // Constant filename for the entire game
    private const string SAVE_FILENAME = "RoomObjectData.json";
    
    // Dictionary for fast lookup: InstanceID -> RoomObjectData
    private Dictionary<int, RoomObjectData> trackedObjects = new Dictionary<int, RoomObjectData>();
    
    // Cache for saved data to apply when objects register
    private Dictionary<string, Queue<RoomObjectData>> _savedStateCache;

    // List of IDs that were explicitly deleted and should be destroyed on load
    private HashSet<string> _loadedDeletedIDs;
    private HashSet<string> _runtimeDeletedIDs = new HashSet<string>();

    private bool _isLoading = false;
    private bool _isQuitting = false;

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
        if (_isQuitting) return;
        SaveRoomState();
    }

    private void OnApplicationQuit()
    {
        _isQuitting = true;
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
        
        // PHASE 4: PARENT LOGIC RESTORATION (Post-Spawn)
        // Now that everyone is spawned, we must re-attach items to their "Gravity Parents" (Stacking).
        // If "Apple" was saved while child of "Table", we find "Table" now and attach "Apple".
        RestoreObjectHierarchy();

        _isLoading = false;
        _savedStateCache = null; // Cleanup
    }
    
    // NEW: Restore Parent-Child Relationships


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
        string sceneName = SceneManager.GetActiveScene().name;
        return $"{sceneName}/{this.name}/{path}";
    }

    private RoomPanel GetOwnerPanel(Transform objTr)
    {
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
        
        // CHECK EXPLICIT DELETION FIRST
        // If this ID is in the "Deleted List" from the loaded state, we must DESTROY it.
        // This handles "Pre-placed objects deleted by player".
        if (_loadedDeletedIDs != null && _loadedDeletedIDs.Contains(uniqueID))
        {
             Debug.Log($"[RoomPanel] KILLING DELETED OBJECT: {obj.name} (ID: {uniqueID}). Found in Deleted List.");
             Destroy(obj);
             return;
        }

        // RoomObject ro = obj.GetComponent<RoomObject>();
        // string rPath = ro != null ? ro.loadedFromResourcePath : "NULL_COMP";
        // Debug.Log($"[RoomPanel] Registering {obj.name}. ID: {uniqueID}. ResourcePath: '{rPath}'");

        // ZOMBIE / CACHE CHECK
        if (_isLoading)
        {
            if (_savedStateCache == null)
            {
                 Debug.LogWarning($"[RoomPanel] Registering {uniqueID} but Cache is NULL!");
            }
            else if (!_savedStateCache.ContainsKey(uniqueID))
            {
                 // LOGIC: If validation (isLoading) is active, AND the object is NOT in the cache,
                 // It might be a new object from editor or a zombie.
                 Debug.LogWarning($"[RoomPanel] Registering '{uniqueID}' - NOT FOUND IN CACHE.");
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
            
            // FIX FOR SCENE OBJECTS: Stop gravity here too!
            if (obj.TryGetComponent<CustomGravity>(out var gravity))
            {
               gravity.DisableAutoStart();
            }
        }
        else
        {
             // Debug.Log($"[RoomPanel] CACHE MISS for {uniqueID}. IsLoading: {_isLoading}.");
             UpdateObjectData(data, obj);
        }

        trackedObjects.Add(id, data);
    }

    public void UnregisterObject(GameObject obj)
    {
        if (_isQuitting) return;

        int id = obj.GetInstanceID();
        if (trackedObjects.ContainsKey(id))
        {
            Debug.Log($"[RoomPanel] Unregistering Object: {obj.name} (ID: {id})");
            
            // TRACK DELETION AT RUNTIME
            RoomObjectData data = trackedObjects[id];
            if (!string.IsNullOrEmpty(data.objectID))
            {
                if (_runtimeDeletedIDs == null) _runtimeDeletedIDs = new HashSet<string>();
                _runtimeDeletedIDs.Add(data.objectID);
            }
            
            trackedObjects.Remove(id);
        }
        else
        {
            Debug.LogWarning($"[RoomPanel] Unregister FAILED. Object {obj.name} (ID: {id}) not found in trackedObjects.");
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
            
            // Save Anchors and Pivot
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
        
        // NEW: Save Parent Information (if attached to another RoomObject)
        if (obj.transform.parent != null && obj.transform.parent != objectContainer) // Ensure we're not just parented to the root container
        {
             // FIX: Use GetComponentInParent to find the Root RoomObject even if attached to a child collider (Surface/Bone)
             // AND INCLUDE INACTIVE: Because sometimes objects are inactive during save/cleanup.
             var parents = obj.transform.parent.GetComponentsInParent<RoomObject>(true);
             RoomObject parentRO = parents.Length > 0 ? parents[0] : null;
             




             // Ensure we don't pick ourselves and ensure it's not the RoomPanel itself (checked generic)
             if (parentRO != null && parentRO.gameObject != obj.gameObject)
             {
                 // We are attached to another item (Stacking!)
                 string pId = GetUniqueID(parentRO.gameObject);
             
             try 
             {
                 data.customStates["attachedParent"] = pId;
             }
             catch (System.Exception e)
             {
                 Debug.LogError($"[RoomPanel] EXCEPTION setting attachedParent: {e}");
             }

                 // NEW: Save the precise transform node (e.g. "Visuals/Surface")

                 // NEW: Save the precise transform node (e.g. "Visuals/Surface")
                 // because parenting to "Table" vs "Table/Surface" changes where LocalPosition puts us.
                 string childPath = GetPathFromAncestor(parentRO.transform, obj.transform.parent);
                 
                 // DEBUG LOGGING
                 /*
                 if (!string.IsNullOrEmpty(childPath))
                 {
                     Debug.Log($"[RoomPanel] DEEP HIERARCHY: {obj.name} -> {parentRO.name} (via '{childPath}')");
                 }
                 */
                 
                 if (!string.IsNullOrEmpty(childPath))
                 {
                     data.customStates["attachedChildPath"] = childPath;
                 }
                 else
                 {
                     data.customStates.Remove("attachedChildPath");
                 }
             }
             else
             {
                 // Parent is null or it is the container
                 data.customStates.Remove("attachedParent");
                 data.customStates.Remove("attachedChildPath");
             }
        }
        else
        {
            // Parent is null or it is the container
             data.customStates.Remove("attachedParent");
             data.customStates.Remove("attachedChildPath");
        }
        
        // Debug.Log($"[RoomPanel] Final States for {obj.name}: {string.Join(", ", data.customStates.Keys)}");
    }
    
    private string GetPathFromAncestor(Transform ancestor, Transform current)
    {
        if (current == ancestor) return "";
        
        string path = current.name;
        Transform ptr = current.parent;
        
        while (ptr != null && ptr != ancestor)
        {
            path = ptr.name + "/" + path;
            ptr = ptr.parent;
        }
        
        if (ptr == null) return ""; // Ancestor not found in chain?
        return path;
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
        if (wrapper.deletedObjectIDs == null) wrapper.deletedObjectIDs = new List<string>();

        // 2. Remove OLD data belonging to THIS RoomPanel in THIS Scene
        string sceneName = SceneManager.GetActiveScene().name;
        string myPrefix = $"{sceneName}/{this.name}/";
        wrapper.objects.RemoveAll(x => x.objectID.StartsWith(myPrefix));
        // Remove old deleted entries for this scope (to be replaced by runtime list)
        // Actually, we should probably merge? But simple replacement prevents stale data buildup for now.
        // wrapper.deletedObjectIDs.RemoveAll(x => x.StartsWith(myPrefix)); 
        // NOTE: Keeping them might be safer, but for now we rely on _runtimeDeletedIDs containing everything that matters.

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
            
            // Check if we are saving new objects
            if (!string.IsNullOrEmpty(data.resourcePath))
            {
                Debug.Log($"[RoomPanel] SAVE ITEM: {obj.name} (ID: {data.objectID}) | Path: {data.resourcePath}");
            }
            else
            {
                Debug.LogWarning($"[RoomPanel] SAVING ITEM WITHOUT PATH: {obj.name} (ID: {data.objectID}). This object will NOT respawn if deleted/missing.");
            }

            UpdateObjectData(data, obj);
            
            // MANUAL SYNC (Refactored): Call the internal method to sync Lists
            data.ForceSync();
            
            newObjects.Add(data);
        }

        wrapper.objects.AddRange(newObjects);
        
        // DEBUG: Audit Tracked Objects
        Debug.Log($"[RoomPanel] --- AUDIT START ({trackedObjects.Count} objects) ---");
        // foreach(var kvp in trackedObjects) {
        //      var d = kvp.Value;
        //      var o = d.instance;
        //      string pName = o != null && o.transform.parent != null ? o.transform.parent.name : "NULL";
        //      Debug.Log($"[Audit] ID: {d.objectID} | Name: {o.name} | Parent: {pName} | InstanceID: {kvp.Key}");
        // }
        Debug.Log($"[RoomPanel] --- AUDIT END ---");

        // 4. Save DELETED Objects
        if (_runtimeDeletedIDs != null && _runtimeDeletedIDs.Count > 0)
        {
            // Simple merge: Add runtime ones, distinct.
            foreach (var delID in _runtimeDeletedIDs)
            {
                if (!wrapper.deletedObjectIDs.Contains(delID) && delID.StartsWith(myPrefix))
                {
                    wrapper.deletedObjectIDs.Add(delID);
                }
            }
            Debug.Log($"[RoomPanel] SAVING DELETED LIST: Total {_runtimeDeletedIDs.Count} runtime deletions processed.");
        }

        PersistenceManager.Save(SAVE_FILENAME, wrapper);
    }
    
    // Internal helper to load state into memory cache
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
        
        // LOAD DELETED IDs
        _loadedDeletedIDs = new HashSet<string>();
        if (wrapper.deletedObjectIDs != null)
        {
            foreach (var id in wrapper.deletedObjectIDs)
            {
                if (id.StartsWith(myPrefix))
                {
                    _loadedDeletedIDs.Add(id);
                    // Also add to runtime list so we preserve old deletions even if we don't re-delete anything
                    _runtimeDeletedIDs.Add(id);
                }
            }
        }
        
        // Flag that we have valid load data
        _isLoading = true;
    }

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

                // PATH RECONSTRUCTION
                 foreach (Transform child in parent)
                 {
                     if (data.objectID.Contains("/" + child.name + "/"))
                     {
                         parent = child;
                         break;
                     }
                 }

                // CRITICAL FIX: Put data in Cache BEFORE Instantiate so RegisterObject finds it immediately.
                if (!_savedStateCache.ContainsKey(data.objectID))
                    _savedStateCache[data.objectID] = new Queue<RoomObjectData>();
                _savedStateCache[data.objectID].Enqueue(data);
                
                Debug.Log($"[RoomPanel {this.GetInstanceID()}] Pre-Spawn Cache Injection for {data.objectID}.");

                GameObject instance = Instantiate(prefabToSpawn, parent);
                
                // CRITICAL FIX: Stop gravity immediately so it doesn't fall while we are restoring data
                if (instance.TryGetComponent<CustomGravity>(out var gravity))
                {
                    gravity.DisableAutoStart();
                }
                
                RoomObject roomObj = instance.GetComponent<RoomObject>();
                if (roomObj == null) 
                {
                    // instance.AddComponent<RoomObject>();
                    // Debug.LogWarning($"[RoomPanel] {instance.name} missing RoomObject.");
                    // SKIP or Continue? If missing, we can't set path.
                }
                else
                {
                   roomObj.loadedFromResourcePath = data.resourcePath;
                }
                
                string[] parts = data.objectID.Split('/');
                if (parts.Length > 0) instance.name = parts[parts.Length - 1]; 
                
                // CRITICAL FIX: Manually register immediately while Cache is valid.
                Debug.Log($"[RoomPanel] Manual Register: {instance.name}");
                RegisterObject(instance);
            }
            else
            {
                Debug.LogWarning($"[RoomPanel] Failed to load resource: {data.resourcePath}");
            }
        }
        else
        {
             Debug.LogWarning($"[RoomPanel] Cannot spawn {data.objectID}: No ResourcePath saved.");
        }
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
            // Restore Anchors/Pivot FIRST
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

            // Force update to ensure anchors take effect before position
            // UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(rect);

            // NEW: Apply SizeDelta (Width/Height) if saved
            if (data.customStates.TryGetValue("sizeDeltaX", out var szX) && data.customStates.TryGetValue("sizeDeltaY", out var szY))
            {
                if (float.TryParse(szX, out float sdX) && float.TryParse(szY, out float sdY))
                {
                    rect.sizeDelta = new Vector2(sdX, sdY);
                }
            }
            
            // Direct apply from stored Position (AnchoredPosition3D)
            rect.anchoredPosition3D = data.position;
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
                    string sourceName = null;
                    if (data.customStates.TryGetValue("interactedSource", out var sName))
                    {
                        sourceName = sName;
                    }
                    interaction.RestoreState(isInteracted, sourceName);
                }
            }
        }
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

    private void RestoreObjectHierarchy()
    {
        Debug.Log($"[RoomPanel] RestoreObjectHierarchy Started. Processing {trackedObjects.Count} objects.");
        
        foreach (var kvp in trackedObjects)
        {
            RoomObjectData data = kvp.Value;
            GameObject childObj = data.instance;

            if (childObj == null) continue;
            
            // Check if this object should have a parent
            if (data.customStates.TryGetValue("attachedParent", out string parentID))
            {
                // Find parent object
                GameObject parentObj = null;
                
                // 1. Try finding in tracked objects (fast, precise)
                foreach(var potentialParent in trackedObjects.Values)
                {
                    if (potentialParent.objectID == parentID && potentialParent.instance != null)
                    {
                        parentObj = potentialParent.instance;
                        break;
                    }
                }
                
                if (parentObj != null)
                {
                    Transform targetParent = parentObj.transform;
                    
                    // DEEP HIERARCHY: Check if we need to attach to a specific child node
                    if (data.customStates.TryGetValue("attachedChildPath", out string childPath))
                    {
                        Transform deepTarget = targetParent.Find(childPath);
                        if (deepTarget != null)
                        {
                            targetParent = deepTarget;
                        }
                        else
                        {
                            Debug.LogWarning($"[RoomPanel] parent found '{parentObj.name}' but failed to find child path '{childPath}'. Defaulting to root.");
                        }
                    }

                    // PERFORM REPARENTING
                    childObj.transform.SetParent(targetParent, true); 
                    
                    if (childObj.name.Contains("Hummer")) Debug.Log($"[RoomPanel] REPARENTED Hummer to {targetParent.name}. Frame: {Time.frameCount}");

                    // CANVAS SYNC: Main Gameobject (childObj) keeps its canvas. 
                    // Children under it take canvas value from it.
                    Canvas mainCanvas = childObj.GetComponent<Canvas>();
                    
                    // If the main object doesn't have a canvas, maybe we should look for one? 
                    // But per user instruction, we look for "Ana Gameobject'in canvası" (Main GameObject's Canvas).
                    // Assuming childObj is the Main GameObject being loaded.
                    
                    if (mainCanvas != null)
                    {
                        var nestedCanvases = childObj.GetComponentsInChildren<Canvas>(true);
                        foreach (var cc in nestedCanvases)
                        {
                             // Skip the main canvas itself
                             if (cc == mainCanvas) continue;
                             
                             cc.overrideSorting = true;
                             cc.sortingLayerID = mainCanvas.sortingLayerID;
                             cc.sortingOrder = mainCanvas.sortingOrder;
                        }
                        if (childObj.name.Contains("Hummer")) Debug.Log($"[RoomPanel] Internal Canvas Sync for {childObj.name}. Sub-canvases set to Order: {mainCanvas.sortingOrder}");
                    }

                    // Reset local transform properties if needed
                    if (childObj.TryGetComponent<RectTransform>(out var rect))
                    {
                        rect.anchoredPosition3D = data.position;
                    }
                    else
                    {
                        childObj.transform.localPosition = data.position;
                    }
                    
                    // CRITICAL: Stop gravity immediately so it doesn't fall off or reparent itself to floor
                    if (childObj.TryGetComponent<CustomGravity>(out var gravity))
                    {
                        gravity.StopFalling();
                    }
                    
                    Debug.Log($"[RoomPanel] RESTORE SUCCESS: {childObj.name} attached to {targetParent.name}");
                }
                else
                {
                    Debug.LogWarning($"[RoomPanel] Restore Failed for {childObj.name}. Target Parent '{parentID}' not found.");
                }
            }
        }
    }
}