using UnityEngine;

public class RoomObject : MonoBehaviour
{
    private RoomPanel _currentRoomPanel;
    private Vector3 _lastPosition;
    private Quaternion _lastRotation;

    private void Start()
    {
        // Find the parent RoomPanel
        _currentRoomPanel = GetComponentInParent<RoomPanel>();
        if (_currentRoomPanel != null)
        {
            _currentRoomPanel.RegisterObject(this.gameObject);
        }
        else
        {
            Debug.LogWarning($"RoomObject {name} is not a child of a RoomPanel!");
        }

        _lastPosition = transform.position;
        _lastRotation = transform.rotation;
    }

    private void OnDestroy()
    {
        if (_currentRoomPanel != null)
        {
            _currentRoomPanel.UnregisterObject(this.gameObject);
        }
    }

    private void Update()
    {
        // Check for transform changes
        if (transform.position != _lastPosition || transform.rotation != _lastRotation)
        {
            NotifyChange();
            _lastPosition = transform.position;
            _lastRotation = transform.rotation;
        }
    }

    public void NotifyChange()
    {
        if (_currentRoomPanel != null)
        {
            _currentRoomPanel.NotifyObjectChanged(this.gameObject);
        }
    }
    
    // Call this if state changes (like open/close)
    public void NotifyStateChange()
    {
         NotifyChange();
    }
}
