using UnityEngine;
using AvatarWorld.House.Furniture;
// using AvatarWorld.House.Furniture.Interaction; // Removed invalid reference
using Assets.Scripts.CharacterAndHouse.CharacterScripts.Expression; // For ExpressionManager

namespace AvatarWorld.Interaction
{
    public class CharacterSleepingController : MonoBehaviour
    {
        [Header("Settings")]
        public float moveSpeed = 10f; // Snap speed (although we usually snap instantly in DragHandler)

        public bool IsSleeping { get; private set; }
        private Bed currentBed;
        private Quaternion defaultRotation;
        private CharacterExpressionManager expressionManager;
        private CustomGravity gravity;

        private void Start()
        {
            defaultRotation = transform.rotation;
            expressionManager = GetComponent<CharacterExpressionManager>();
            gravity = GetComponent<CustomGravity>();
        }

        public void TrySleep(Bed bed)
        {
            if (bed == null) return;

            // 1. Move to Bed Head Position
            if (bed.headPoint != null)
            {
                transform.position = bed.headPoint.position;
            }

            // 2. Rotate Character (Laying down)
            transform.rotation = Quaternion.Euler(0, 0, bed.sleepRotation);

            // 3. Close Eyes (Emotion)
            if (expressionManager != null)
            {
                expressionManager.SetEmotion(EmotionType.Sleep);
            }

            // 4. Disable Gravity
            if (gravity != null) gravity.StopFalling();

            // 5. State
            IsSleeping = true;
            currentBed = bed;

            // NOTE: No parenting. Follow in LateUpdate.
        }

        public void WakeUp()
        {
             if (!IsSleeping) return;

             // 1. Reset Rotation
             transform.rotation = defaultRotation;
             // transform.localScale = Vector3.one; // Not needed if we don't parent

             // 2. Open Eyes (Neutral or Previous?)
             if (expressionManager != null)
             {
                 expressionManager.SetEmotion(EmotionType.Neutral);
             }

             // 3. State
             IsSleeping = false;
             currentBed = null;
        }

        private void LateUpdate()
        {
            if (IsSleeping && currentBed != null)
            {
                 // Follow Bed
                 if (currentBed.headPoint != null)
                 {
                     transform.position = currentBed.headPoint.position;
                 }
                 else
                 {
                     transform.position = currentBed.transform.position;
                 }
            }
        }
    }
}
