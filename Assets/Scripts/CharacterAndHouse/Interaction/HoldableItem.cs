using UnityEngine;
using UnityEngine.UI;

namespace AvatarWorld.Interaction
{
    //Eşyaların "tutulabilir" olduğunu belirten etiket.
    public class HoldableItem : MonoBehaviour
    {
        [Header("Holding Settings")]
        public bool isTwoHanded = false;
        public Vector3 holdOffset = Vector3.zero;
        public Vector3 holdRotation = Vector3.zero;

        [Header("State")]
        public bool isHeld = false;
        
        // Cache original state if needed
        private CustomGravity customGravity;
        private Canvas canvas;
        private int originalSortingOrder;

        private void Awake()
        {
            customGravity = GetComponent<CustomGravity>();
            canvas = GetComponent<Canvas>();
        }

        public void OnGrab()
        {
            isHeld = true;
            if (customGravity != null)
            {
                customGravity.StopFalling();
                // Disable gravity update so it doesn't fight the hand position
                customGravity.enabled = false; 
            }
            
            // AUTO-FIX: Ensure we have a Canvas to control Sorting Order
            if (canvas == null) canvas = GetComponent<Canvas>();
            if (canvas == null) canvas = gameObject.AddComponent<Canvas>();
            
            // Ensure we have a GraphicRaycaster so we can be clicked!
            if (GetComponent<UnityEngine.UI.GraphicRaycaster>() == null)
            {
                gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }

            // FORCE RENDER ON TOP
            canvas.overrideSorting = true;
            canvas.sortingOrder = 100; // High value to sit above Character (usually 0-10)
            
            Debug.Log($"[HoldableItem] OnGrab: Forced SortingOrder 100 on {name}");
        }

        public void OnRelease()
        {
            isHeld = false;
            if (customGravity != null)
            {
                customGravity.enabled = true;
                customGravity.StartFalling();
            }

            // Reset sorting or let it be handled by the room
        }
    }
}
