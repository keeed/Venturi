using System;
using Xer.Cqrs.EventStack;

namespace Domain.DomainEvents
{
    public class ProductUnregisteredEvent : IEvent
    {
        public Guid InventoryId { get; }
        public Guid ProductId { get; }

        public ProductUnregisteredEvent(Guid productId, Guid inventoryId)
        {
            ProductId = productId;
            InventoryId = inventoryId;
        }
    }
}