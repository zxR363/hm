using UnityEngine;

public class SlidePanelButton : MonoBehaviour
{
    [SerializeField] private SlidePanelController slidePanelController;

    public void OnClickToggle()
    {
        if (slidePanelController == null)
        {
            slidePanelController = FindObjectOfType<SlidePanelController>();
        }

        if (slidePanelController != null)
        {
            slidePanelController.TogglePanel();
        }
        else
        {
            Debug.LogError("[SlidePanelButton] SlidePanelController reference is missing and could not be found!");
        }
    }
}