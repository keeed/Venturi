using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain;
using Domain.Repositories;

namespace Infrastructure.Repositories
{
    public class InMemoryInventoryRepository : IInventoryRepository
    {
        private readonly List<Inventory> _inventories = new List<Inventory>()
        {
            new Inventory(new InventoryId(Guid.Parse("b97ec9f8-4ca5-4d69-b1e7-a02fc1cb38de")), new WarehouseId(Guid.NewGuid()))
        };
        
        public Task<Inventory> GetByIdAsync(InventoryId inventoryId, CancellationToken ct = default(CancellationToken))
        {
            if (inventoryId == null)
            {
                throw new System.ArgumentNullException(nameof(inventoryId));
            }

            var result = _inventories.FirstOrDefault(i => i.Id == inventoryId);
            return Task.FromResult(new Inventory(result)); // Make a copy to return as result.
        }

        public Task SaveAsync(Inventory inventory, CancellationToken ct = default(CancellationToken))
        {
            if (inventory == null)
            {
                throw new System.ArgumentNullException(nameof(inventory));
            }

            _inventories.RemoveAll(i => i.Id == inventory.Id);

            _inventories.Add(inventory);

            return Task.CompletedTask;
        }
    }
}