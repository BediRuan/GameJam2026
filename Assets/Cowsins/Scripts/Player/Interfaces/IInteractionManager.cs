namespace cowsins2D
{
    using UnityEngine;
    public interface IInteractionManager
    {
        float TimeToInteract {  get; }
        void DropInventoryItem(InventorySlot inventorySlot);
    }
}