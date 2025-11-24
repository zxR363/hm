using UnityEngine;
using DG.Tweening;

public class CloudMovements : MonoBehaviour
{
    void Start()
    {
        AnimateDeleteButtonImage();
    }

    private void AnimateDeleteButtonImage()
    {
        //+- 10 derece ,10 sn de tamamlıyor
        transform.DOLocalRotate(new Vector3(0f, 0f, 10f), 10f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }
}
