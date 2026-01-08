using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("SettingsButton Area")]
    [SerializeField] private GameObject settingsView;
    [SerializeField] private GameObject groupMainMenu; 
    [SerializeField] private AudioSource audioSource;   
    
    private bool isAudioActive = true;

    private CanvasGroup mainMenuCanvasGroup;

    private void Awake()
    {
        if (groupMainMenu != null)
        {
            mainMenuCanvasGroup = groupMainMenu.GetComponent<CanvasGroup>();
            if (mainMenuCanvasGroup == null)
            {
#if UNITY_EDITOR
                // READ-ONLY PATTERN
                /*
                UnityEditor.EditorApplication.delayCall += () => {
                   // ...
                };
                */
#else
                // mainMenuCanvasGroup = groupMainMenu.AddComponent<CanvasGroup>();
#endif
                 // Debug.LogWarning("[MainMenuController] MainMenu CanvasGroup missing.");
            }
        }

    }

    public void StartGame()
    {
        SceneLoader.LoadSceneWithTransition("BuildingSelectionScene");
    }   

    public void OpenShop()
    {
        SceneLoader.LoadSceneWithTransition("ShopScene");
    }

    public void OpenSettingsView()
    {
        settingsView.SetActive(true);
        
        if (mainMenuCanvasGroup != null)
        {
            mainMenuCanvasGroup.blocksRaycasts = false;
        }
    }

    public void CloseSettingsView()
    {
        settingsView.SetActive(false);
        
        if (mainMenuCanvasGroup != null)
        {
            mainMenuCanvasGroup.blocksRaycasts = true;
        }
    }

    public void ToggleAudio(bool isOn)
    {
        if (isAudioActive)
        {
            if (audioSource != null)
            {
                audioSource.mute = true;    
            }
            isAudioActive = false;
        }
        else
        {
            if (audioSource != null)
            {
                audioSource.mute = false;
            }
            isAudioActive = true;
        }
    }

}
