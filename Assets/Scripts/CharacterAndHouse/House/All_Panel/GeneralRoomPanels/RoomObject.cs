using UnityEngine;

/*
1.Kayıt (Registration):
Nesne sahneye eklendiğinde (Start), otomatik olarak hangi RoomPanel'in
içinde olduğunu bulur ve kendini ona kaydettirir (RegisterObject).
Yok olduğunda (OnDestroy) kaydını sildirir.

2.Değişiklik Takibi (Change Detection):
Nesnenin pozisyonu veya rotasyonu değiştiğinde (Update içinde kontrol eder),
 bağlı olduğu odaya "Ben değiştim" sinyali gönderir (NotifyChange).

3.Kaydetme Sistemi İçin Tetikleyici:
Bu sinyaller sayesinde RoomPanel, içerisindeki eşyaların durumunun
 değiştiğini anlar ve muhtemelen Save System (Kaydetme Sistemi)
  için verileri güncellemesi gerektiğini bilir.
*/

public class RoomObject : MonoBehaviour
{
    private RoomPanel _currentRoomPanel;
    private Vector3 _lastPosition;
    private Quaternion _lastRotation;

    [SerializeField] private int defaultSortingOrder = 20;

    private void Start()
    {
        // Apply default sorting order if Canvas exists
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.overrideSorting = true;
            canvas.sortingOrder = defaultSortingOrder;
        }

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

    public void NotifyChange(bool saveNow = false)
    {
        if (_currentRoomPanel != null)
        {
            _currentRoomPanel.NotifyObjectChanged(this.gameObject, saveNow);
        }
    }
    
    // Call this if state changes (like open/close)
    public void NotifyStateChange()
    {
         NotifyChange(true);
    }
}
