using UnityEngine;


//Her panelde kayıtedilen Gameobjectlerin' spesifik özellikleri saklanmak istenirse bu kullanılır(RoomPanel içerisinde)

public class ObjectState : MonoBehaviour
{
    public bool IsOpen = false;
    public float Temperature = 22.5f;

    private RoomPanel roomPanel;

    private void Start()
    {
        roomPanel = GetComponentInParent<RoomPanel>();
    }

    public void ToggleOpen()
    {
        IsOpen = !IsOpen;
        roomPanel?.NotifyObjectChanged(gameObject);
    }

    public void SetTemperature(float value)
    {
        Temperature = value;
        roomPanel?.NotifyObjectChanged(gameObject);
    }
}