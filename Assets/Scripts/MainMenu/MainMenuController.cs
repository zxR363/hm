using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public void StartGame()
    {
        SceneLoader.LoadSceneWithTransition("BuildingSelectionScene");
    }

    public void OpenSettings()
    {
        SceneLoader.LoadSceneWithTransition("SettingsScene");
    }

}
