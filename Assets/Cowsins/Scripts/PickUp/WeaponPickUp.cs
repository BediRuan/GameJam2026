using UnityEngine;
using UnityEngine.Events;

namespace cowsins2D
{
    public class WeaponPickUp : PickUp
    {
        [Tooltip("Weapon_SO to pick up. If using hotbar only inventory, this will go to your hotbar. " +
            "In case you are using the full inventory and your hotbar is full, it will be placed inside your inventory ( if there is still space ). " +
            "In case the hotbar is not full it will be placed in a free slot. ")]
        [SerializeField] private Weapon_SO weapon;

        [HideInInspector] public int currentBullets, totalBullets;

        [HideInInspector] public bool dropped;

        private UnityEvent onPickUp;

        protected override void Start()
        {
            base.Start();
            SetPickeableGraphics(weapon);

            // This gets called only if the player did not drop this pick up.
            if (dropped) return;
            currentBullets = weapon.magazineSize;
            totalBullets = weapon.amountOfMagazines * weapon.magazineSize;
        }

        public override string GetInteractText()
        {
            string text = includeNamePickUpText ? $"{interactText} {weapon?.itemName}" : interactText;
            return text;
        }

        public override void Interact(PlayerDependencies source)
        {
            // Do not forget to always pass InteractionManager as it is required
            if (source == null) return;

            onPickUp?.Invoke();

            IInventoryManager inventoryManager = source.InventoryManager;

            if (inventoryManager.InventoryMethod == InventoryMethod.HotbarOnly)
                BasicInventory(source);
            else FullInventory(source);
        }

        #region BASIC_INVENTORY
        private void BasicInventory(PlayerDependencies source)
        {
            IWeaponController weaponController = source.WeaponController;

            if (weaponController == null) return;

            if (!HotbarIsFull(source))
            {
                // The hotbar is not full, so there is no need to keep this pickeable, destroy it
                Destroy(this.gameObject);
                return;
            }

            weaponController.InstantiateWeapon(weapon, weaponController.currentWeapon, currentBullets, totalBullets);

            // Because the inventory is full, we have to override the graphics of this weapon pickeable
            SetPickeableGraphics(weapon);
        }

        private void FullInventory(PlayerDependencies source)
        {
            IWeaponController weaponController = source.WeaponController;

            if (weaponController == null) return;

            if (!HotbarIsFull(source))
            {
                // The hotbar is not full, so there is no need to keep this pickeable, destroy it
                Destroy(this.gameObject);
                return;
            }

            UIController UIController = source._UIController;

            // Hotbar is full, check if inventory is full.
            if (UIController.IsInventoryFull())
            {
                return;
            }

            var wp = Instantiate(weapon.weaponObject, weaponController.WeaponHolder.position, weaponController.WeaponHolder.rotation, weaponController.WeaponHolder);

            wp.currentBullets = currentBullets;
            wp.totalBullets = totalBullets;
            wp.gameObject.SetActive(false);

            UIController.PopulateInventoryWithWeapon(weapon, 1, currentBullets, totalBullets);

            Destroy(this.gameObject);
        }

        private bool HotbarIsFull(PlayerDependencies playerDependencies)
        {
            IWeaponController weaponController = playerDependencies.WeaponController;
            IInventoryManager inventoryManager = playerDependencies.InventoryManager;

            // Check for any empty slot
            for (int i = 0; i < inventoryManager.HotbarSize; i++)
            {
                // Is this an empty slot? 
                if (weaponController.inventory[i] == null)
                {
                    weaponController.InstantiateWeapon(weapon, i, currentBullets, totalBullets);
                    playerDependencies._UIController.UpdateWeaponInformation();
                    return false;
                }

            }
            // There are no empty slots.        
            return true;
        }

        #endregion

        #region OTHERS

        public void AssignWeapon(Weapon_SO newWeapon)
        {
            weapon = newWeapon;
        }

        #endregion
    }
}