using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class TransitionController : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(LoadNextScene());
    }

    IEnumerator LoadNextScene()
    {
        yield return new WaitForSeconds(3f); // Geçiş süresi

        SceneManager.LoadScene(SceneLoader.nextSceneName);
    }
}
