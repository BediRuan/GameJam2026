using UnityEngine;

namespace cowsins2D
{
    public class PlayerControlLock : MonoBehaviour
    {
        public static PlayerControlLock Instance { get; private set; }

        [Header("Runtime (Read Only)")]
        public bool inputLocked = false;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void SetLocked(bool locked)
        {
            inputLocked = locked;
        }
    }
}
