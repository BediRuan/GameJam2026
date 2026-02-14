using UnityEngine;
using UnityEngine.Events;

namespace cowsins2D
{
    public class ItemPickUp : PickUp
    {
        [Tooltip("Amount to pick up, these will be stored in the Inventory in case you use a full inventory system.")] public int amount;

        [Tooltip("Item_SO to pick up.")] public Item_SO item;

        private UnityEvent onPickUp;

        protected override void Start()
        {
            base.Start();
            SetPickeableGraphics(item);
        }

        public override string GetInteractText()
        {
            string text = includeNamePickUpText ? $"{interactText} {item?.itemName}" : interactText;
            return text;
        }

        public override void Interact(PlayerDependencies source)
        {
            // Perform Basic or Full operations depending on the Player�s current inventory system selected.

            // Get a reference to the player WeaponController.
            IWeaponController wc = source.WeaponController;
            IInventoryManager inventoryManager = source.InventoryManager;

            switch (inventoryManager.InventoryMethod)
            {
                case InventoryMethod.HotbarOnly:
                    BasicInteraction(source);
                    break;
                case InventoryMethod.Full:
                    FullInteraction(source);
                    break;
            }

            //Perform custom interaction operations.
            onPickUp?.Invoke();
        }

        private void BasicInteraction(PlayerDependencies playerDependencies)
        {
            // Use the item on interact.
            item.Use(playerDependencies);
            Destroy(this.gameObject);
        }

        private void FullInteraction(PlayerDependencies playerDependencies)
        {
            UIController UIController = playerDependencies._UIController;
            // Checks if the inventory is full. If it is, the item will not be picked up.
            if (UIController.IsInventoryFull()) return;

            // Calculates the remaining amount of the item after picking it up.
            int remainingAmount = amount;

            // While there is still remaining amount, add it to the inventory.
            while (remainingAmount > 0)
            {
                int ammoToAdd = Mathf.Min(remainingAmount, item.maxStack);
                UIController.PopulateInventory(item, ammoToAdd);
                remainingAmount -= ammoToAdd;
            }

            Destroy(this.gameObject);
        }
    }
}