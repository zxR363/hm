using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance;

    [SerializeField] public List<RoomPanel> roomPanelsList;

    private void Awake()
    {
        Instance = this;
    }

    public void LoadRoom(RoomType roomType)
    {
        // Save current state before switching? 
        // Ideally we save all active rooms or just the one being hidden.
        SaveAllRooms();
        
        RoomLoader.Load(roomType, roomPanelsList);
    }

    public void SaveAllRooms()
    {
        foreach (var panel in roomPanelsList)
        {
            if (panel.gameObject.activeSelf)
            {
                panel.SaveRoomState();
            }
        }
    }
    
    private void OnApplicationQuit()
    {
        SaveAllRooms();
    }
    
    private void OnApplicationPause(bool pause)
    {
        if (pause) SaveAllRooms();
    }
}