using UnityEngine;

namespace AvatarWorld.House.Furniture
{
    public class Seat : MonoBehaviour
    {
        [Header("Seat Settings")]
        [Tooltip("Where the character's pivot (hips) should snap to.")]
        public Transform sitPoint;

        [Tooltip("If true, faces right when sitting. If false, faces left.")]
        public bool faceRight = true;

        private void OnDrawGizmos()
        {
            if (sitPoint != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(sitPoint.position, 10f);
            }
        }
    }
}
