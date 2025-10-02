using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlidePanelItemButton : MonoBehaviour
{
    public void OnClick()
    {
        ItemSelectionPanelController.Instance.TogglePanel();
    }
}

