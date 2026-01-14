using UnityEngine;

namespace AvatarWorld.House.Furniture
{
    public class Bed : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("Where the character's HEAD should be when sleeping.")]
        public Transform headPoint;

        [Tooltip("Rotation for the sleeping character. Usually 90 or -90.")]
        public float sleepRotation = 90f;

        private void OnDrawGizmos()
        {
            if (headPoint != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(headPoint.position, 10f); // Head visualization
                Gizmos.DrawLine(headPoint.position, headPoint.position + Vector3.right * 50f); // Direction
            }
        }
    }
}
