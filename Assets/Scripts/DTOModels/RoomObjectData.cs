using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class RoomObjectData
{
    public string objectID; // Prefab veya obje adı
    public Vector3 position;
    public Quaternion rotation;
    public Dictionary<string, string> customStates = new Dictionary<string, string>();
    public GameObject instance; // Sahnedeki obje referansı
}