using UnityEngine;
using UnityEngine.Events;

namespace cowsins2D
{
    public class PlayerControl : MonoBehaviour, IPlayerControl
    {
        public PlayerControlEvents PlayerControlEvents { get; set; } = new PlayerControlEvents();
        public bool Controllable { get; private set; }

        private PlayerDependencies playerDependencies;
        private IPlayerStats playerStats;
        private IInventoryManager inventoryManager;
        private Rigidbody2D rb;

        private void Awake()
        {
            playerDependencies = GetComponent<PlayerDependencies>();
            playerStats = playerDependencies.PlayerStats;
            inventoryManager = playerDependencies.InventoryManager;
            rb = playerDependencies.Rigidbody;
            GrantControl();
        }

        public void GrantControl()
        {
            Controllable = true;
            PlayerControlEvents.onGrantControl?.Invoke();
            rb.linearDamping = 0;
        }

        public void LoseControl()
        {
            Controllable = false;
            PlayerControlEvents.onLoseControl?.Invoke();
            rb.linearDamping = 2;
        }

        public void ToggleControl()
        {
            Controllable = !Controllable;
            if (Controllable) PlayerControlEvents.onGrantControl?.Invoke();
            else PlayerControlEvents.onLoseControl?.Invoke();
        }

        public void CheckIfCanGrantControl()
        {
            if (playerStats.IsDead || inventoryManager.InventoryOpen) return;

            GrantControl();
        }
    }
}