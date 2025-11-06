using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBuildingSelector : MonoBehaviour
{
    public BuildingManager buildingManager;

    //UI Butonlarına Bu fonksiyon bağlanır.
    //Butona tıklanıldığında index'deki binanın yapımı gerçekleşiyor.
    public void OnSelectBuilding(int index)
    {
        buildingManager.SelectBuilding(index);
    }
}

