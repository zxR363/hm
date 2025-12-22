using UnityEngine;

public class CustomGravity : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float gravitySpeed = 60.0f; // Units per second (Increased for UI)
    [Tooltip("Layers to consider as 'Ground' (Solid stops)")]
    [SerializeField] private LayerMask groundLayer; 

    private bool _isFalling = false;
    private Collider2D _myCollider;
    private ContactFilter2D _contactFilter;
    private RaycastHit2D[] _raycastResults = new RaycastHit2D[10]; // Buffer

    private void Awake()
    {
        _myCollider = GetComponent<Collider2D>();
        
        // Setup filter to check everything initially, or restrict if needed
        _contactFilter.useTriggers = true; 
        _contactFilter.useLayerMask = true;
        _contactFilter.layerMask = Physics2D.AllLayers; // We scan all, but filter below
        // Debug.Log($"[CustomGravity] Initialized. Ground Layer: {groundLayer.value}");
    }

    private void Start()
    {
        // USER REQUEST: Start falling immediately if not grounded
        StartFalling(); 
    }

    public void StartFalling()
    {
        // USER REQUEST: If already on an ILocation, do NOT start gravity logic.
        // Check exact center point for an ILocation
        Collider2D hit = Physics2D.OverlapPoint(transform.position, _contactFilter.layerMask);
        if (hit != null)
        {
             bool isLocation = hit.GetComponent<ILocation>() != null || hit.GetComponentInParent<ILocation>() != null;
             if (isLocation)
             {
                 // We are safe. Don't fall, don't snap. Just stay.
                 _isFalling = false;
                 Debug.Log($"[CustomGravity] Already on Location {hit.name}. Fall Cancelled.");
                 return;
             }
        }

        _isFalling = true; // Start falling
    }

    public void StopFalling()
    {
        _isFalling = false;
    }

    private void FixedUpdate()
    {
        if (_isFalling)
        {
            HandleFall();
        }
    }

    private void HandleFall()
    {
        if (_myCollider == null) 
        {
            Debug.LogError($"[CustomGravity] Collider is NULL on {name}! Cannot fall.");
            return;
        }

        float moveAmount = gravitySpeed * Time.fixedDeltaTime; 
        // Debug.Log($"[CustomGravity] Falling... Move: {moveAmount:F2} Y: {transform.position.y:F2}"); 
        Vector3 potentialPos = transform.position;
        potentialPos.y -= moveAmount;

        // Check if we have a valid collider to use for dimensions
        Vector2 boxSize = _myCollider.bounds.size;
        // SCRUB-FIX: Shrink width slightly (10%) to avoid grazing vertical walls while falling
        boxSize.x *= 0.9f; 

        int hitCount = Physics2D.BoxCast(
            _myCollider.bounds.center, 
            boxSize, 
            0f, 
            Vector2.down, 
            _contactFilter, 
            _raycastResults, 
            moveAmount + 0.05f 
        );

        if (hitCount > 0)
        {
            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit2D hit = _raycastResults[i];
                
                // Ignore self and children
                if (hit.collider.gameObject == gameObject) continue;
                if (hit.transform.IsChildOf(transform)) continue; 

                // Check if it's a valid stopping point:
                // 1. ILocation (Slot/Room Location)
                bool isLocation = hit.collider.GetComponent<ILocation>() != null;
                if (!isLocation) isLocation = hit.collider.GetComponentInParent<ILocation>() != null;

                // 2. Solid Ground Check (Must be in Ground Layer AND Not Trigger)
                bool isSolidGround = false;
                
                // Check Layer Mask
                if (((1 << hit.collider.gameObject.layer) & groundLayer) != 0)
                {
                    if (!hit.collider.isTrigger)
                    {
                         isSolidGround = true;

                         // DEBUG: See what we are hitting
                         // Debug.Log($"[CustomGravity] Considering {hit.collider.name}. Normal: {hit.normal} PointY: {hit.point.y} MyY: {transform.position.y}");

                         // 1. Normal Check (Must face UP)
                         // 0.5f ~ 45 degrees. If < 0.5f, it's a wall or ceiling.
                         if (hit.normal.y < 0.5f) 
                         {
                             isSolidGround = false; 
                         }
                         
                         // 2. Position Check (Hit Point must be below Center)
                         // Prevents stopping on side-walls that we graze
                         if (hit.point.y > transform.position.y)
                         {
                             isSolidGround = false;
                         }
                    }
                }

                if (isLocation || isSolidGround)
                {
                    // Landed!
                    _isFalling = false;
                    Debug.Log($"[CustomGravity] Landed on {hit.collider.name}. (Loc: {isLocation}, Gnd: {isSolidGround}, Norm: {hit.normal})");
                    return;
                }
            }
        }

        // Apply movement if no collision
        transform.position = potentialPos;
    }
}
