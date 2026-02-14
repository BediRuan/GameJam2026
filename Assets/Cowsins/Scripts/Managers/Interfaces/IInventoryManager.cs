namespace cowsins2D
{
    using System;
    using UnityEngine;

    public interface IInventoryManager
    {
        bool InventoryOpen { get; }
        int HotbarSize { get; }
        bool DropOnOutsideRelease { get; }
        InventoryMethod InventoryMethod { get; }

        void CloseInventory();

        Action toggleInventory { get; }
    }
}