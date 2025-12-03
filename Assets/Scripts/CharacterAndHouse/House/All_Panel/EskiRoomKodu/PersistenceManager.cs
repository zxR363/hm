using System.IO;
using UnityEngine;

public static class PersistenceManager
{
    public static void Save<T>(string filename, T data)
    {
        string path = Path.Combine(Application.persistentDataPath, filename);
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);
        Debug.Log($"Saved data to {path}");
    }

    public static T Load<T>(string filename)
    {
        string path = Path.Combine(Application.persistentDataPath, filename);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<T>(json);
        }
        else
        {
            Debug.LogWarning($"Save file not found at {path}");
            return default(T);
        }
    }
    
    public static bool Exists(string filename)
    {
        return File.Exists(Path.Combine(Application.persistentDataPath, filename));
    }
}
