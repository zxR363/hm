using UnityEngine;

public class SlidePanelButton : MonoBehaviour
{
    [SerializeField] private SlidePanelController slidePanelController;

    public void OnClickToggle()
    {
        slidePanelController.TogglePanel();
    }
}