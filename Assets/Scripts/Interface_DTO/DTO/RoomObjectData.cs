using UnityEngine;
using System.Collections.Generic;
using System;

[System.Serializable]
public class RoomObjectData : ISerializationCallbackReceiver
{
    public string objectID; // Prefab veya obje adı
    public Vector3 position;
    public Quaternion rotation;
    
    [System.NonSerialized]
    public Dictionary<string, string> customStates = new Dictionary<string, string>();

    // Serialization helpers
    public List<string> _stateKeys = new List<string>();
    public List<string> _stateValues = new List<string>();

    public GameObject instance; // Sahnedeki obje referansı

    public void OnBeforeSerialize()
    {
        _stateKeys.Clear();
        _stateValues.Clear();
        foreach(var kvp in customStates)
        {
            _stateKeys.Add(kvp.Key);
            _stateValues.Add(kvp.Value);
        }
    }

    public void OnAfterDeserialize()
    {
        customStates = new Dictionary<string, string>();
        for(int i = 0; i < Math.Min(_stateKeys.Count, _stateValues.Count); i++)
        {
            customStates[_stateKeys[i]] = _stateValues[i];
        }
    }
}