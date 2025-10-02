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
        RoomLoader.Load(roomType,roomPanelsList); // ← sadece tetikler
    }


}