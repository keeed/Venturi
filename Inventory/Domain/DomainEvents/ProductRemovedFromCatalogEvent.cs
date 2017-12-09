using System;
using Xer.Cqrs.EventStack;

namespace Domain.DomainEvents
{
    public class ProductRemovedFromCatalogEvent : IEvent
    {
        public Guid InventoryId { get; }
        public Guid ProductId { get; }
        public string CatalogName { get; }

        public ProductRemovedFromCatalogEvent(Guid inventoryId, Guid productId, string catalogName)
        {
            InventoryId = inventoryId;
            ProductId = productId;
            CatalogName = catalogName;
        }
    }
}