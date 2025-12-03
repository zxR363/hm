using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Bina yapım alanı için kullanılıyor.
public class IBuildingSlots : MonoBehaviour
{    
    [Header("EmptyBuilding Görsel Ayarları (posx,posy,posz)")]
    public Vector3 emptyBuildingPosition; //posX,posY,posZ
    public Vector2 emptyBuildingSize; //width,height
    public Vector3 emptyBuildingRotation;

    [Header("BuiltBuilding Görsel Ayarları(width,height)")]
    public Vector3 builtBuildingPosition;//posX,posY,posZ
    public Vector2 builtBuildingSize;//width,height
    public Vector3 builtBuildingRotation;
}
