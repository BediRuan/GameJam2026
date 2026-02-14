using UnityEngine;

namespace cowsins2D
{
    /// <summary>
    /// Ladder component to manage ladder bounds and climb limits
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class Ladder : MonoBehaviour
    {
        [Header("Ladder Configuration")]
        [Tooltip("Percentage from the top where climbing is blocked (0-1). 0.05 = last 5% of ladder height.")]
        [SerializeField, Range(0f, 0.3f)] private float topExitThreshold = 0.08f;

        [SerializeField] private bool showDebugGizmos = true;

        private BoxCollider2D ladderCollider;
        private float ladderHeight;
        private float exitZoneY;

        private void Awake()
        {
            ladderCollider = GetComponent<BoxCollider2D>();
            CalculateLadderBounds();
        }

        private void OnValidate()
        {
            if (ladderCollider == null) ladderCollider = GetComponent<BoxCollider2D>();
            CalculateLadderBounds();
        }

        private void CalculateLadderBounds()
        {
            if (ladderCollider == null) return;

            // Get ladder bounds
            Bounds bounds = ladderCollider.bounds;
            ladderHeight = bounds.size.y;
            
            // Calculate y position where climb is blocked
            float topY = bounds.max.y;
            exitZoneY = topY - (ladderHeight * topExitThreshold);
        }

        /// <summary>
        /// Check if the player can continue climbing upward from their current position
        /// Returns true if the player is below the exit zone
        /// </summary>
        public bool CanClimbHigher(Vector2 playerPosition)
        {
            return playerPosition.y < exitZoneY;
        }

        /// <summary>
        /// Check if the player is in the exit zone ( Calcualted from the top! )
        /// </summary>
        public bool IsInExitZone(Vector2 playerPosition)
        {
            return playerPosition.y >= exitZoneY;
        }

        public float GetTopY()
        {
            return ladderCollider.bounds.max.y;
        }

        public float GetBottomY()
        {
            return ladderCollider.bounds.min.y;
        }

        private void OnDrawGizmos()
        {
            if (!showDebugGizmos || ladderCollider == null) return;

            // Draw the top/blocked zone in red
            Bounds bounds = ladderCollider.bounds;
            Vector3 center = new Vector3(bounds.center.x, exitZoneY + (bounds.max.y - exitZoneY) * 0.5f, bounds.center.z);
            Vector3 size = new Vector3(bounds.size.x, bounds.max.y - exitZoneY, bounds.size.z);
            
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawCube(center, size);

            // Draw the allowed ladder zone in green
            Vector3 climbCenter = new Vector3(bounds.center.x, (bounds.min.y + exitZoneY) * 0.5f, bounds.center.z);
            Vector3 climbSize = new Vector3(bounds.size.x, exitZoneY - bounds.min.y, bounds.size.z);
            
            Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
            Gizmos.DrawCube(climbCenter, climbSize);

            Gizmos.color = Color.yellow;
            Vector3 lineStart = new Vector3(bounds.min.x, exitZoneY, bounds.center.z);
            Vector3 lineEnd = new Vector3(bounds.max.x, exitZoneY, bounds.center.z);
            Gizmos.DrawLine(lineStart, lineEnd);
        }
    }
}
