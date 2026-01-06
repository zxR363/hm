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
    
    // Stores the Resources path this object was loaded from (for Persistence)
    public string loadedFromResourcePath;

    private void Start()
    {
        // OPTIMIZATION: Disable Animator by default to prevent "Graphic Rebuild Loop" (500Hz)
        // If an object needs animation (like Fridge opening), its controller must enable it explicitly.
        Animator anim = GetComponent<Animator>();
        if (anim != null)
        {
            anim.enabled = false;
        }
        // Also check children (e.g. Button_Fridge -> Image_Fridge)
        // BE CAREFUL: Don't disable child animators if they are critical? 
        // For now, let's stick to the root or known sub-parts.
        
        // Apply default sorting order if Canvas exists
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.overrideSorting = true;
            canvas.sortingOrder = defaultSortingOrder;
        }

        // Find the parent RoomPanel
        _currentRoomPanel = GetComponentInParent<RoomPanel>();
        //Debug.Log($"[RoomObject] {name} INITIALIZED. Parent Panel: {(_currentRoomPanel != null ? _currentRoomPanel.name : "NULL")}");

        if (_currentRoomPanel != null)
        {
            _currentRoomPanel.RegisterObject(this.gameObject);
        }
        else
        {
            Debug.LogWarning($"[RoomObject] {name} is not a child of a RoomPanel! Detection Failed.");
        }

        _lastPosition = transform.position;
        _lastRotation = transform.rotation;
    }

    private void OnDestroy()
    {
        //Debug.Log($"[RoomObject] OnDestroy called for {name}. Unregistering from Panel: {(_currentRoomPanel != null ? _currentRoomPanel.name : "NULL")}");
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

    private void OnTransformParentChanged()
    {
        // When parent changes, we MUST check if the RoomPanel has changed.
        // Previous logic ignored this if _currentRoomPanel was not null, which is wrong if we moved to a new Panel.
        
        RoomPanel newPanel = GetComponentInParent<RoomPanel>();
        
        if (newPanel != _currentRoomPanel)
        {
            // Unregister from old
            if (_currentRoomPanel != null)
            {
                 _currentRoomPanel.UnregisterObject(this.gameObject);
            }
            
            // Register to new
            _currentRoomPanel = newPanel;
            if (_currentRoomPanel != null)
            {
                 Debug.Log($"[RoomObject] {name} parent changed. New Panel: {_currentRoomPanel.name}. Registering... Path: '{loadedFromResourcePath}'");
                 _currentRoomPanel.RegisterObject(this.gameObject);
            }
        }
    }

    public void NotifyChange(bool saveNow = false)
    {
        //Debug.Log($"[RoomObject] NotifyChange called on {name}. SaveNow: {saveNow}. Panel: {_currentRoomPanel}");
        if (_currentRoomPanel != null)
        {
            _currentRoomPanel.NotifyObjectChanged(this.gameObject, saveNow);
        }
        else
        {
             // Retry finding panel?
             _currentRoomPanel = GetComponentInParent<RoomPanel>();
             if (_currentRoomPanel != null)
                _currentRoomPanel.NotifyObjectChanged(this.gameObject, saveNow);
             else
             {
                 // Do nothing. This happens when:
                 // 1. Object is being dragged (Ghost) -> No RoomPanel yet.
                 // 2. Object is just instantiated before parenting.
                 // Debug.LogWarning($"[RoomObject] NotifyChange ignored. No RoomPanel found on {name}.");
             }
        }
    }
    
    // Call this if state changes (like open/close)
    public void NotifyStateChange()
    {
         NotifyChange(true);
    }
}
