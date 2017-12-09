using System;

namespace Domain
{
    public class Warehouse // Aggregate
    {
        public WarehouseId Id { get; private set; }
        public InventoryId InventoryId { get; private set; }
        public string Name { get; private set; }

        public Warehouse(WarehouseId warehouseId, InventoryId inventoryId, string warehouseName)
        {
            Id = warehouseId;
            InventoryId = inventoryId;
            Name = warehouseName;
        }

        public Warehouse(WarehouseId warehouseId, string warehouseName)
            : this(warehouseId, new InventoryId(Guid.Empty), warehouseName)
        {
            Id = warehouseId;
            Name = warehouseName;
        }

        public Inventory CreateNewInventory()
        {
            return new Inventory(new InventoryId(Guid.NewGuid()), Id);
        }
    }
}