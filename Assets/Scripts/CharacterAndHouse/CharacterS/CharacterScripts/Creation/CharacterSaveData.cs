using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CharacterPartSaveData
{
    public string partName;
    public string spriteName;
    public string folderPath;
    public string colorHex = "#FFFFFF";
}

[Serializable]
public class CharacterSaveData
{
    public string charId;
    public List<CharacterPartSaveData> parts = new List<CharacterPartSaveData>();
}
