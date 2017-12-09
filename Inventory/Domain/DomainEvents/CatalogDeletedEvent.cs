using System;
using Xer.Cqrs.EventStack;

namespace Domain.DomainEvents
{
    public class CatalogDeletedEvent : IEvent
    {
        public Guid InventoryId { get; }
        public string CatalogName { get; }

        public CatalogDeletedEvent(Guid inventoryId, string catalogName)
        {
            InventoryId = inventoryId;
            CatalogName = catalogName;
        }
    }
}