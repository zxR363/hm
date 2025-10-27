using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ItemsColorList", menuName = "Character/ItemsColorList")]
public class ItemsColorList : ScriptableObject
{
    public List<Color> colors;
}
