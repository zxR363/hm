using System.Collections.Generic;
using UnityEngine;

public class RoomButton : MonoBehaviour
{
    public RoomType targetRoom;
    

    public void OnClick()
    {
        Debug.Log("RoomButton clicked");        
        RoomManager.Instance.LoadRoom(targetRoom);
    }

    public void test()
    {
        Debug.Log("TEST");
    }
}