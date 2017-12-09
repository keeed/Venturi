using System;
using System.Collections.Generic;

namespace Domain
{
    public class Catalog // Value object
    {
        public InventoryId InventoryId { get; private set; }
        public string Name { get; private set; }

        public Catalog(InventoryId inventoryId, string name)
        {
            InventoryId = inventoryId;
            Name = name;
        }
    }
}