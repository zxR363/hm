using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DeletedObjectManager : MonoBehaviour
{
    private static DeletedObjectManager _instance;
    public static DeletedObjectManager Instance 
    { 
        get 
        {
            if (_instance == null)
            {
                // Check if it exists in scene but reference was lost
                _instance = FindObjectOfType<DeletedObjectManager>();
                
                if (_instance == null)
                {
                    // Create it
                    GameObject obj = new GameObject("DeletedObjectManager");
                    _instance = obj.AddComponent<DeletedObjectManager>();
                }
            }
            return _instance;
        } 
    }

    private const string FILE_NAME = "deleted_objects.json";
    private HashSet<string> _deletedIds = new HashSet<string>();
    private string _filePath;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            _filePath = Path.Combine(Application.persistentDataPath, FILE_NAME);
            Load();
        }
        else if (_instance != this)
        {
            // If the auto-created one (or another one) already exists, destroy this duplicate
            Destroy(gameObject);
            return;
        }
        
        // Ensure path is set if we woke up normally
        if (string.IsNullOrEmpty(_filePath))
        {
             _filePath = Path.Combine(Application.persistentDataPath, FILE_NAME);
             Load();
        }
    }

    public bool IsDeleted(string id)
    {
        if (string.IsNullOrEmpty(id)) return false;
        return _deletedIds.Contains(id);
    }

    public void MarkAsDeleted(string id)
    {
        if (string.IsNullOrEmpty(id)) return;
        if (!_deletedIds.Contains(id))
        {
            _deletedIds.Add(id);
            Save();
        }
    }

    private void Save()
    {
        SerializationWrapper wrapper = new SerializationWrapper();
        wrapper.ids = new List<string>(_deletedIds);
        string json = JsonUtility.ToJson(wrapper, true);
        File.WriteAllText(_filePath, json);
        Debug.Log($"[DeletedObjectManager] Saved {_deletedIds.Count} deleted items.");
    }

    private void Load()
    {
        if (File.Exists(_filePath))
        {
            try
            {
                string json = File.ReadAllText(_filePath);
                SerializationWrapper wrapper = JsonUtility.FromJson<SerializationWrapper>(json);
                if (wrapper != null && wrapper.ids != null)
                {
                    _deletedIds = new HashSet<string>(wrapper.ids);
                    Debug.Log($"[DeletedObjectManager] Loaded {_deletedIds.Count} deleted items.");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[DeletedObjectManager] Failed to load data: {e.Message}");
            }
        }
    }

    [System.Serializable]
    private class SerializationWrapper
    {
        public List<string> ids;
    }
}
