using System;
using UnityEngine;
using UnityEngine.Events;

namespace cowsins2D
{
    [System.Serializable]
    public enum InventoryMethod
    {
        None, HotbarOnly, Full
    }
    public class InventoryManager : MonoBehaviour, IInventoryManager
    {
        [System.Serializable]
        public class InventoryManagerEvents
        {
            public UnityEvent onToggleInventory, onOpenInventory, onCloseInventory;
        }

        [SerializeField] private InventoryMethod inventoryMethod;

        [Min(1), SerializeField] private int hotbarSize;

        [SerializeField, Tooltip("Reference to the inventory object.")] private GameObject inventory;

        [SerializeField, Tooltip("Number of rows and columns, the total number of items is rows*columns.")] private int inventoryRowsAmount, inventoryColumnsAmount;

        [SerializeField, Tooltip("If enabled, whe the Inventory is opened, the player can drop items outside the " +
            "inventory by releasing them outside the bounds of the Inventory Container")]
        private bool dropOnOutsideRelease = true;

        [SerializeField, Tooltip("Sounds played on opening & closing the inventory. ")] private AudioClip openInventorySFX, closeInventorySFX;

        [SerializeField] private InventoryManagerEvents inventoryManagerEvents;

        public InventoryMethod InventoryMethod => inventoryMethod;
        public int HotbarSize => hotbarSize;
        public int InventorySize => inventoryRowsAmount * inventoryColumnsAmount;
        public bool DropOnOutsideRelease => dropOnOutsideRelease;

        public Action toggleInventory { get; private set; }

        private bool inventoryOpen = false;
        public bool InventoryOpen => inventoryOpen;

        private IPlayerControl playerControl;
        private IWeaponController weaponController;
        private UIController UIController;
        private PlayerDependencies playerDependencies;
        private InputManager inputManager;

        private void Start()
        {
            playerDependencies = GetComponent<PlayerDependencies>();
            playerControl = playerDependencies.PlayerControl;
            weaponController = playerDependencies.WeaponController;
            UIController = playerDependencies._UIController;
            inputManager = playerDependencies.InputManager;

            weaponController.WeaponControllerEvents.onShoot.AddListener(UpdateCurrentWeaponBullets);
            weaponController.WeaponControllerEvents.onStopReload.AddListener(UpdateCurrentWeaponBullets);

            playerDependencies.InputManager.onOpenInventory += PerformToggleInventoryEvent;

            PopulateInitialInventory();
            InitializeInventory();
        }

        private void OnDisable()
        {
            weaponController.WeaponControllerEvents.onShoot.RemoveListener(UpdateCurrentWeaponBullets);
            weaponController.WeaponControllerEvents.onStopReload.RemoveListener(UpdateCurrentWeaponBullets);

            playerDependencies.InputManager.onOpenInventory -= PerformToggleInventoryEvent;
        }

        private void Update()
        {
            HandleHotBar();
            if (inventoryOpen) UIController.HandleInventoryNavigation();
        }

        private void PopulateInitialInventory()
        {
            if (weaponController.InitialWeapons.Length == 0) return;
            int i = 0;
            while (i < weaponController.InitialWeapons.Length)
            {
                Weapon_SO weaponToCheck = weaponController.InitialWeapons[i];
                weaponController.InstantiateWeapon(weaponToCheck, i);
                i++;
            }
        }

        private void InitializeInventory()
        {
            // Checks what methods to use depending on the inventory method
            switch (inventoryMethod)
            {
                case InventoryMethod.None: break;

                case InventoryMethod.HotbarOnly:
                    UIController.InitializeHotBar(hotbarSize);
                    break;

                case InventoryMethod.Full:
                    UIController.InitializeHotBar(hotbarSize);
                    UIController.InitializeFullInventory(SlotType.Inventory, inventoryRowsAmount, inventoryColumnsAmount, ref UIController.inventorySlots);
                    toggleInventory = ToggleFullInventory;
                    break;
            }

            if(inventory !=  null) inventory.SetActive(false);
        }
        private void HandleHotBar()
        {
            // You cannot select a weapon while reloading
            if (weaponController == null || weaponController.reloading || !playerControl.Controllable) return;

            // Based on the mouse wheel input, change the weapon
            float scrollDelta = inputManager.PlayerInputs.MouseWheel.y;
            bool next = scrollDelta > 0 || inputManager.PlayerInputs.NextWeapon;
            bool prev = scrollDelta < 0 || inputManager.PlayerInputs.PreviousWeapon;

            if (next && weaponController.currentWeapon < hotbarSize - 1)
            {
                weaponController.currentWeapon++;
                weaponController.UnholsterWeapon();
                UIController.updateHotbarSelection?.Invoke();
            }
            else if (prev && weaponController.currentWeapon > 0)
            {
                weaponController.currentWeapon--;
                weaponController.UnholsterWeapon();
                UIController.updateHotbarSelection?.Invoke();
            }
        }

        private void PerformToggleInventoryEvent()
        {
            if (!playerControl.Controllable && inventory.activeSelf == true || playerControl.Controllable)
                toggleInventory?.Invoke();
        }

        private void ToggleFullInventory()
        {
            if(PauseMenu.isPaused) return;

            bool shouldOpen = !inventory.gameObject.activeSelf;

            if (shouldOpen) OpenInventory();
            else CloseInventory();

            inventoryManagerEvents.onToggleInventory?.Invoke();
        }

        public void OpenInventory()
        {
            inventoryOpen = true;
            inventory.SetActive(true);

            playerControl.LoseControl();

            SoundManager.Instance.PlaySound(openInventorySFX, 1);
            Crosshair.Instance.Hide(true);

            UIController.SetHighlightedSlot(UIController.inventorySlots[0,0], false);
            UIController.HideInteractionUI();

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            inventoryManagerEvents.onOpenInventory?.Invoke();
        }

        public void CloseInventory()
        {
            inventoryOpen = false;
            inventory.SetActive(false);
            playerControl.CheckIfCanGrantControl();

            UIController.currentInventorySlot = null;
            UIController.highlightedInventorySlot?.OnPointerExit(null);
            UIController.highlightedInventorySlot = null;
            UIController.CloseChest();

            Tooltip.Instance.Hide();
            SoundManager.Instance.PlaySound(closeInventorySFX, 1);
            Cursor.visible = false;
            if (Crosshair.Instance.CheckIfCanShow()) Crosshair.Instance.Show();

            inventoryManagerEvents.onCloseInventory?.Invoke();
        }

        private void UpdateCurrentWeaponBullets()
        {
            WeaponIdentification id = weaponController.id;
            if (id == null) return;

            UIController.OverrideHotbarSlotBullets(weaponController.currentWeapon, id.currentBullets, id.totalBullets); 
        }
    }
}