using UnityEngine;
using AvatarWorld.House.Furniture;

namespace AvatarWorld.Interaction
{
    public class CharacterSittingController : MonoBehaviour
    {
        [Header("Sitting Settings (Visual Polish)")]
        [Tooltip("Spread angle (Open legs). User Req: 25.")]
        public float sitRotationAngle = 40f; 
        
        [Tooltip("Scale Y for foreshortening. User Req: 0.88.")]
        public float sitLegScaleY = 1f;

        [Tooltip("Offset legs position to simulate better pivot point. Try (0, 10, 0) to move them UP.")]
        public Vector3 sitLegPositionOffset = new Vector3(-5, 10f, 0);

        [Tooltip("Global rotation for both legs to face one side (Skew). Positive = Left, Negative = Right.")]
        public float sitSideSkewAngle = -20f; 

        [Header("References (Auto-Found)")]
        public Transform leftLeg;
        public Transform rightLeg;

        private Quaternion defaultLeftLegRot;
        private Quaternion defaultRightLegRot;
        private Vector3 defaultLeftLegScale;
        private Vector3 defaultRightLegScale;
        private Vector3 defaultLeftLegPos;
        private Vector3 defaultRightLegPos;
        
        public bool IsSitting { get; private set; }
        private Seat currentSeat;

        private void Start()
        {
            // Auto-find Legs using rigorous recursive search
            if (leftLeg == null) leftLeg = FindPartRecursively(transform, "LeftLeg");
            if (rightLeg == null) rightLeg = FindPartRecursively(transform, "RightLeg");

            // Cache default rotations and scales
            if (leftLeg != null) 
            {
                defaultLeftLegRot = leftLeg.localRotation;
                defaultLeftLegScale = leftLeg.localScale;
                defaultLeftLegPos = leftLeg.localPosition;
            }
            if (rightLeg != null) 
            {
                defaultRightLegRot = rightLeg.localRotation;
                defaultRightLegScale = rightLeg.localScale;
                defaultRightLegPos = rightLeg.localPosition;
            }
        }

        public void TrySit(Seat seat)
        {
            if (seat == null) return;
            
            // 1. Move to seat
            if (seat.sitPoint != null)
            {
                transform.position = seat.sitPoint.position;
            }

            // 2. Rotate/Scale Legs
            RotateLegsForSitting(true);

            // 3. State
            IsSitting = true;
            currentSeat = seat;

            // 4. Disable Gravity
            var gravity = GetComponent<CustomGravity>();
            if (gravity != null) gravity.StopFalling();
        }

        public void StandUp()
        {
            if (!IsSitting) return;

            // 1. Reset Legs
            RotateLegsForSitting(false);

            // 2. State
            IsSitting = false;
            currentSeat = null;
        }

        private void RotateLegsForSitting(bool sitting)
        {
            if (leftLeg == null || rightLeg == null) return;

            if (sitting)
            {
                // POLISH: Foreshortening + Side Skew + Pivot Offset
                
                // 1. Calculate Rotations
                float leftZ = sitRotationAngle + sitSideSkewAngle;
                float rightZ = -sitRotationAngle + sitSideSkewAngle;

                // Apply
                leftLeg.localRotation = Quaternion.Euler(0, 0, leftZ);
                rightLeg.localRotation = Quaternion.Euler(0, 0, rightZ);

                // 2. Squash Y axis 
                Vector3 targetScale = new Vector3(1f, sitLegScaleY, 1f); 
                leftLeg.localScale = Vector3.Scale(defaultLeftLegScale, targetScale);
                rightLeg.localScale = Vector3.Scale(defaultRightLegScale, targetScale);
                
                // 3. Apply Offset (Simulate Pivot Change)
                leftLeg.localPosition = defaultLeftLegPos + sitLegPositionOffset;
                rightLeg.localPosition = defaultRightLegPos + sitLegPositionOffset;
            }
            else
            {
                // Reset to default
                leftLeg.localRotation = defaultLeftLegRot;
                rightLeg.localRotation = defaultRightLegRot;
                leftLeg.localScale = defaultLeftLegScale;
                rightLeg.localScale = defaultRightLegScale;
                leftLeg.localPosition = defaultLeftLegPos;
                rightLeg.localPosition = defaultRightLegPos;
            }
        }
        
        private Transform FindPartRecursively(Transform parent, string name)
        {
            if (parent.name == name) return parent;
            foreach (Transform child in parent)
            {
                Transform found = FindPartRecursively(child, name);
                if (found != null) return found;
            }
            return null;
        }
    }
}
