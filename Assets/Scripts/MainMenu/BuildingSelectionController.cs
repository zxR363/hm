using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingSelectionController : MonoBehaviour
{
    public void GoBackToMainMenu()
    {
        SceneLoader.LoadSceneWithTransition("MainMenuScene");
    }

}
