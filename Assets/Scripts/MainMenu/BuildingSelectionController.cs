using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class BuildingSelectionController : MonoBehaviour
{

    public void GoBackToMainMenu()
    {
        SceneLoader.LoadSceneWithTransition("MainMenuScene");
    }

    public void LoadHouse()
    {
        SceneLoader.LoadSceneWithTransition("HouseScene");
    }
    
    public void LoadSchool()
    {
        SceneLoader.LoadSceneWithTransition("TestHouse");
    }

    public void BuildingSelectionScene()
    {
        SceneLoader.LoadSceneWithTransition("BuildingSelectionScene");
    }


    //-----------------KARAKTER SCENE'E GECIS ALANI---------------------
    public void GoToCharacterScene()
    {
        PlayerPrefs.SetString("PreviousScene", SceneManager.GetActiveScene().name);
        SceneManager.LoadScene("CharacterScene");
    }

    public void BackCharacterToPreviousScene()
    {
        string previousScene = PlayerPrefs.GetString("PreviousScene", "HouseScene");
        SceneManager.LoadScene(previousScene);
    }
    //-----------------KARAKTER SCENE'E GECIS ALANI---------------------

}
