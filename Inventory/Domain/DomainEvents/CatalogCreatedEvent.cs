using System;
using Xer.Cqrs.EventStack;

namespace Domain.DomainEvents
{
    public class CatalogCreatedEvent : IEvent
    {
        public Guid InventoryId { get; }
        public string CatalogName { get; }

        public CatalogCreatedEvent(Guid inventoryId, string catalogName)
        {
            InventoryId = inventoryId;
            CatalogName = catalogName;
        }
    }
}