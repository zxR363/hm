using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "SkinColorList", menuName = "Character/SkinColorList")]
public class SkinColorList : ScriptableObject
{
    public List<Color> colors;
}
