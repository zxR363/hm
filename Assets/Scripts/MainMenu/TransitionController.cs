using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;


public class TransitionController : MonoBehaviour
{

    [SerializeField] private Image transitionImage;
    
    Sprite randomSprite;
    
    void Start()
    {
        StartCoroutine(LoadNextScene());
    }

    IEnumerator LoadNextScene()
    {
        // 1️⃣ Rastgele görsel seç
        transitionImage.sprite = GetRandomTransitionSprite();

        yield return new WaitForSeconds(3f); // Geçiş süresi

        SceneManager.LoadScene(SceneLoader.nextSceneName);
    }


    //RASTGELE RESIM SECIYOR
    private static Sprite GetRandomTransitionSprite()
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>("Images/TransitionImages");
        if (sprites == null || sprites.Length == 0)
        {
            Debug.LogWarning("No transition images found in Resources/Images/TransitionImages");
            return null;
        }

        int index = Random.Range(0, sprites.Length);
        return sprites[index];
    }
}
