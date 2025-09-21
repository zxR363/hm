using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public GameObject kitchen;
    public GameObject livingRoom;
    public GameObject bedroom;

    public void ShowKitchen()
    {
        kitchen.SetActive(true);
        livingRoom.SetActive(false);
        bedroom.SetActive(false);
    }

    public void ShowLivingRoom()
    {
        kitchen.SetActive(false);
        livingRoom.SetActive(true);
        bedroom.SetActive(false);
    }

    public void ShowBedroom()
    {
        kitchen.SetActive(false);
        livingRoom.SetActive(false);
        bedroom.SetActive(true);
    }
}
