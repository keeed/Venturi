using System;
using System.Collections.Generic;
using System.Linq;
using Domain.DomainEvents;
using Xer.Cqrs.EventStack;

namespace Domain
{
    public class Inventory : IEventSource, IStateContainer<InventoryState> // Aggregate
    {
        // These events will be published when entity is saved using PublishingRepository
        private readonly List<IEvent> _events = new List<IEvent>();
        private readonly List<Catalog> _catalogs;

        private InventoryId _inventoryId;
        private readonly WarehouseId _warehouseId;

        public InventoryId Id => _inventoryId;

        #region IEventSource Implementation

        #region IStateContainer Implementation

        public InventoryState GetCurrentState()
        {
            return new InventoryState(_inventoryId, _warehouseId, _catalogs);
        }

        #endregion IStateContainer Implementation

        IEnumerable<IEvent> IEventSource.GetEvents() => _events.AsReadOnly();
        void IEventSource.ClearEvents() => _events.Clear();

        #endregion IEventSource Implementation

        public Inventory(InventoryId inventoryId, WarehouseId warehouseId)
        {
            _inventoryId = inventoryId;
            _warehouseId = warehouseId;
            _catalogs = new List<Catalog>();
        }

        public Inventory(InventoryId inventoryId, WarehouseId warehouseId, IEnumerable<Catalog> catalogs)
        {
            _inventoryId = inventoryId;
            _warehouseId = warehouseId;
            _catalogs = catalogs.ToList();
        }

        public void RegisterNewProduct(ProductId productId, string productName, string productDescription, decimal productPrice)
        {
            // Create product instance in event handler.
            _events.Add(new ProductRegisteredEvent(productId.Value,
                                                   _inventoryId.Value,
                                                   productName,
                                                   productDescription,
                                                   productPrice));
        }

        public void UnregisterProduct(ProductId productId)
        {
            // Delete product instance in event handler.
            _events.Add(new ProductUnregisteredEvent(productId.Value, _inventoryId.Value));
        }

        public void CreateNewCatalog(string catalogName)
        {
            if (_catalogs.Any(c => c.Name == catalogName))
            {
                throw new InvalidOperationException($"{catalogName} already exists in inventory.");
            }

            _catalogs.Add(new Catalog(_inventoryId, catalogName));

            _events.Add(new CatalogCreatedEvent(_inventoryId.Value, catalogName));
        }

        public void DeleteCatalog(string catalogName)
        {
            _catalogs.RemoveAll(c => c.Name == catalogName);

            _events.Add(new CatalogDeletedEvent(_inventoryId.Value, catalogName));
        }

        public void AddProductToCatalog(ProductId productId, string catalogName)
        {
            Catalog catalog = EnsureCatalogExists(catalogName);

            _events.Add(new ProductAddedToCatalogEvent(Id.Value, productId.Value, catalogName));
        }

        public void RemoveProductFromCatalog(ProductId productId, string catalogName)
        {
            Catalog catalog = EnsureCatalogExists(catalogName);

            _events.Add(new ProductRemovedFromCatalogEvent(Id.Value, productId.Value, catalogName));
        }

        private Catalog EnsureCatalogExists(string catalogName)
        {
            Catalog catalog = _catalogs.FirstOrDefault(c => c.Name == catalogName);
            if (catalog == null)
            {
                throw new InvalidOperationException("Catalog does not exist in inventory.");
            }
            return catalog;
        }
    }

    #region Inventory State

    public class InventoryState
    {
        public InventoryState(InventoryId inventoryId, WarehouseId warehouseId, IEnumerable<Catalog> catalogs)
        {
            InventoryId = inventoryId;
            WarehouseId = warehouseId;
            Catalogs = catalogs.ToList().AsReadOnly();
        }

        public InventoryId InventoryId { get; }
        public WarehouseId WarehouseId { get; }
        public IReadOnlyCollection<Catalog> Catalogs { get; set; }
    }

    #endregion Inventory State
}