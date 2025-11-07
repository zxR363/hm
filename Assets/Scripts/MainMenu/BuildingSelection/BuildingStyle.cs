using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingStyle : MonoBehaviour
{
    public EnumBuildingType type;

    public void ApplyStyle()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        Color color = type switch
        {
            EnumBuildingType.House => Color.blue,
            EnumBuildingType.Factory => Color.gray,
            EnumBuildingType.Farm => Color.green,
            _ => Color.white
        };

        foreach (Renderer r in renderers)
        {
            r.material.color = color;
        }
    }
}

