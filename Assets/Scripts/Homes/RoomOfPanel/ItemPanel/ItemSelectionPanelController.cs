using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSelectionPanelController : MonoBehaviour
{
    public static ItemSelectionPanelController Instance;

    [SerializeField] private GameObject panelRoot;
    [SerializeField] private List<TabButton> tabButtons;
    [SerializeField] private List<GameObject> tabContents;

    private bool isActive=false;

    private void Awake()
    {
        Instance = this;
        panelRoot.SetActive(false);
    }

    public void TogglePanel()
    {
        if(isActive==false)
        {
            isActive=true;
            OpenPanel();
        }
        else
        {
            isActive=false;
            ClosePanel();
        }
    }

    public void OpenPanel()
    {
        panelRoot.SetActive(true);
        SelectTab(0); // Varsayılan ilk tab
    }

    public void ClosePanel()
    {
        panelRoot.SetActive(false);
    }

    public void SelectTab(int index)
    {
        Debug.Log("SELECT TAB TIKLANIYOR INDEX="+index);
        for (int i = 0; i < tabContents.Count; i++)
        {
            tabContents[i].SetActive(i == index);
        }
    }
    
}
