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

    public void LoadRoom(RoomType roomType)
    {
        RoomLoader.Load(roomType,roomPanels); // ← sadece tetikler
    }


}