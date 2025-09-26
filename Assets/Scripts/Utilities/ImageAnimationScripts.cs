using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageAnimationScripts : MonoBehaviour
{
    [SerializeField] private ContainerController itemButtonEvent;

    public void OnContainerClosing()
    {
        itemButtonEvent.OnContainerClosing();
        Debug.Log("ImageAnimation closing animation event triggered.");
        // Buraya animasyon sonrası yapılacak işlemleri yazabilirsin
    }


    public void OnContainerOpened()
    {
        itemButtonEvent.OnContainerOpened();
        Debug.Log("ImageAnimation opening animation event triggered.");
    }


}
