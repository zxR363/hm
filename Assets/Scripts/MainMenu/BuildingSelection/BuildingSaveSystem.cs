using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class BuildingSaveSystem
{
    private const string SaveFileName = "building_state.json";
    //Kayıt işlemi Assets/Databases/BuildingDB
    private static string FullPath => Path.Combine(Application.dataPath, "Databases/BuildingDB/building_state.json");
    
    //Kayıt işlemi Application.persistentDataPath altında yapılıyor, yani
    //C:/Users/[KullanıcıAdı]/AppData/LocalLow/[CompanyName]/[ProductName]/building_state.json
    //private static string FullPath => Path.Combine(Application.persistentDataPath, SaveFileName);

    public static void SaveSlotState(string slotID, bool isBuilt)
    {
        Debug.Log("SAVE SLOT TETIKLENDI");
        var data = LoadAll();
        data[slotID] = isBuilt;

        try
        {
            // ✅ klasörü oluştur. - Dosya yazmadan önce klasörün varlığını garanti eder
            Directory.CreateDirectory(Path.GetDirectoryName(FullPath));
            string json = JsonUtility.ToJson(new SerializableBoolDict(data), true);
            File.WriteAllText(FullPath, json);
        }
        catch (IOException e)
        {
            Debug.LogError($"❌ Save failed: {e.Message}");
        }
    }

    public static bool TryGetSlotState(string slotID, out bool isBuilt)
    {
        isBuilt = false;

        if (!File.Exists(FullPath))
            return false;

        try
        {
            string json = File.ReadAllText(FullPath);
            if (string.IsNullOrEmpty(json))
                return false;

            var dict = JsonUtility.FromJson<SerializableBoolDict>(json);
            if (dict == null || dict.keys == null || dict.values == null)
                return false;

            var data = dict.ToDictionary();
            return data.TryGetValue(slotID, out isBuilt);
        }
        catch (IOException e)
        {
            Debug.LogError($"❌ Load failed: {e.Message}");
            return false;
        }
    }

    private class SerializableBoolDict
    {
        public List<string> keys = new();
        public List<bool> values = new();

        public SerializableBoolDict() { }

        public SerializableBoolDict(Dictionary<string, bool> dict)
        {
            foreach (var kvp in dict)
            {
                keys.Add(kvp.Key);
                values.Add(kvp.Value);
            }
        }

        public Dictionary<string, bool> ToDictionary()
        {
            var dict = new Dictionary<string, bool>();
            int count = Mathf.Min(keys.Count, values.Count);

            for (int i = 0; i < count; i++)
                dict[keys[i]] = values[i];

            return dict;
        }
    }

    private static Dictionary<string, bool> LoadAll()
    {
        if (!File.Exists(FullPath))
            return new Dictionary<string, bool>();

        try
        {
            string json = File.ReadAllText(FullPath);
            var dict = JsonUtility.FromJson<SerializableBoolDict>(json);
            return dict?.ToDictionary() ?? new Dictionary<string, bool>();
        }
        catch (IOException e)
        {
            Debug.LogError($"❌ LoadAll failed: {e.Message}");
            return new Dictionary<string, bool>();
        }
    }
}