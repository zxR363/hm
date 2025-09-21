using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    public static string nextSceneName;

    public static void LoadSceneWithTransition(string targetScene)
    {
        nextSceneName = targetScene;
        SceneManager.LoadScene("TransitionScene");
    }
}
