using System.Collections.Generic;
using UnityEngine;

public class SubPanelButtons : MonoBehaviour
{

    public void OnClickCharacter()
    {
       SceneLoader.LoadSceneWithTransition("CharacterScene");
    }

    public void test()
    {
        Debug.Log("TEST");
    }
}