using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabButton : MonoBehaviour
{
    [SerializeField] private int tabIndex;

    public void OnClick()
    {
        ItemSelectionPanelController.Instance.SelectTab(tabIndex);
    }
}
