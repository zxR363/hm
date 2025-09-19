using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("BuildingSelectionScene");
    }

    public void OpenSettings()
    {
        Debug.Log("Ayarlar açıldı.");
        // Ayarlar panelini aktif et
    }

    public void OpenCharacterCreator()
    {
        SceneManager.LoadScene("CharacterCreationScene");
    }
}
