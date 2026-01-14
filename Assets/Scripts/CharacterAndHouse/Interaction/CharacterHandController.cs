using UnityEngine;

namespace AvatarWorld.Interaction
{
    //Karakterin ellerini yöneten beyin.
    public class CharacterHandController : MonoBehaviour
    {
        public Transform leftHandSlot;
        public Transform rightHandSlot;
        
        [Header("State")]
        public HoldableItem heldItemLeft;
        public HoldableItem heldItemRight;

        [Header("Animation Settings")]
        [Tooltip("How many degrees to lift the arm when holding an item.")]
        public float armLiftAngle = 10f;
        [Tooltip("Speed of the arm rotation smoothing.")]
        public float smoothSpeed = 13f;

        private Quaternion defaultRotLeft;
        private Quaternion defaultRotRight;
        private Quaternion targetRotLeft;
        private Quaternion targetRotRight;
        
        private Transform leftArm;
        private Transform rightArm;

        private void Start()
        {
            // Auto-find slots if not assigned
            if (leftHandSlot == null) leftHandSlot = FindHandSlot("HandSlot_L");
            if (rightHandSlot == null) rightHandSlot = FindHandSlot("HandSlot_R");

            // AUTO-FIX: Ensure hands are rendered ON TOP of the body (Last Sibling)
            if (leftHandSlot != null) leftHandSlot.SetAsLastSibling();
            if (rightHandSlot != null) rightHandSlot.SetAsLastSibling();

            // Cache Arms (Parents of slots) and their default rotations
            if (leftHandSlot != null)
            {
                leftArm = leftHandSlot.parent;
                if (leftArm != null) 
                {
                    defaultRotLeft = leftArm.localRotation;
                    targetRotLeft = defaultRotLeft;
                }
            }
            if (rightHandSlot != null)
            {
                rightArm = rightHandSlot.parent;
                if (rightArm != null) 
                {
                    defaultRotRight = rightArm.localRotation;
                    targetRotRight = defaultRotRight;
                }
            }
        }

        private void Update()
        {
            // Smooth Rotation Logic
            if (leftArm != null)
            {
                leftArm.localRotation = Quaternion.Slerp(leftArm.localRotation, targetRotLeft, Time.deltaTime * smoothSpeed);
            }
            if (rightArm != null)
            {
                rightArm.localRotation = Quaternion.Slerp(rightArm.localRotation, targetRotRight, Time.deltaTime * smoothSpeed);
            }
        }

        private Transform FindHandSlot(string slotName)
        {
            foreach (Transform t in GetComponentsInChildren<Transform>(true))
            {
                if (t.name == slotName) return t;
            }
            return null;
        }

        public bool TryHoldItem(HoldableItem item)
        {
            if (item == null) return false;

            // Calculate distances to determine preference
            float distToLeft = Vector3.Distance(item.transform.position, leftHandSlot.position);
            float distToRight = Vector3.Distance(item.transform.position, rightHandSlot.position);

            // Determine preference (Closest hand)
            bool preferRight = distToRight < distToLeft;

            // Scenario 1: Preferred hand is empty -> Grab
            if (preferRight && heldItemRight == null)
            {
                AttachToHand(item, rightHandSlot, isRight: true);
                return true;
            }
            if (!preferRight && heldItemLeft == null)
            {
                AttachToHand(item, leftHandSlot, isRight: false);
                return true;
            }

            // Scenario 2: Preferred hand is full, check the other one
            if (preferRight && heldItemLeft == null) // Wanted Right, but full -> Use Left
            {
                AttachToHand(item, leftHandSlot, isRight: false);
                return true;
            }
            if (!preferRight && heldItemRight == null) // Wanted Left, but full -> Use Right
            {
                AttachToHand(item, rightHandSlot, isRight: true);
                return true;
            }
            
            // Both hands full
            return false;
        }

        private void AttachToHand(HoldableItem item, Transform handSlot, bool isRight)
        {
            if (item == null || handSlot == null) return;

            // Notify item it's being grabbed
            item.OnGrab();

            // Parenting
            item.transform.SetParent(handSlot);
            item.transform.localPosition = item.holdOffset; // Use offset defined in item
            item.transform.localRotation = Quaternion.Euler(item.holdRotation);
            item.transform.localScale = Vector3.one; // Reset scale or keep? Usually reset.

            if (isRight) heldItemRight = item;
            else heldItemLeft = item;
            
            // Rotate Arm (Visual Feedback)
            SetArmRotation(isRight, true);
            
            Debug.Log($"Character grabbed {item.name} with {(isRight ? "Right" : "Left")} hand.");
        }

        public void DropItem(HoldableItem item)
        {
            if (item == heldItemRight)
            {
                heldItemRight = null;
                item.OnRelease();
                SetArmRotation(true, false); // Reset Right Arm
            }
            else if (item == heldItemLeft)
            {
                heldItemLeft = null;
                item.OnRelease();
                SetArmRotation(false, false); // Reset Left Arm
            }
        }

        private void SetArmRotation(bool isRight, bool isHolding)
        {
             // Direction reversed based on user feedback (Ters yönlerde açılıyor)
             // Previous: Left(+), Right(-)
             // New: Left(-), Right(+)
            
            if (isRight)
            {
                // Right Arm
                float finalAngle = isHolding ? armLiftAngle : 0f; // Reversed sign
                targetRotRight = defaultRotRight * Quaternion.Euler(0, 0, finalAngle);
            }
            else
            {
                // Left Arm
                float finalAngle = isHolding ? -armLiftAngle : 0f; // Reversed sign
                targetRotLeft = defaultRotLeft * Quaternion.Euler(0, 0, finalAngle);
            }
        }
    }
}
