using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance;

    [SerializeField] private List<RoomPanel> roomPanels;

    private void Awake()
    {
        Instance = this;
    }

    public void SwitchToRoom(RoomType target)
    {
        foreach (var panel in roomPanels)
        {
            panel.gameObject.SetActive(panel.roomType == target);
        }

        Debug.Log("Switched to room: " + target);
    }
}