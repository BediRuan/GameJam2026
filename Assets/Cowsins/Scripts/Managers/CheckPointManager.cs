using UnityEngine;

namespace cowsins2D
{
    public class CheckPointManager : MonoBehaviour
    {
        [SerializeField] private bool useInitialIfNull;

        public bool UseInitialIfNull => useInitialIfNull;

        // Stores the player's last checkpoint position.
        public Vector3? lastCheckpoint { get; private set; } = null;

        public void SetCheckpoint(Transform obj)
        {
            // Store a new position as the last checkpoint.
            lastCheckpoint = obj.position;
        }
    }
}