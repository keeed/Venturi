using System;
using Xer.Cqrs.EventStack;

namespace Domain.DomainEvents
{
    public class ProductUnregisteredEvent : IEvent
    {
        public Guid InventoryId { get; }
        public Guid ProductId { get; }

        public ProductUnregisteredEvent(Guid inventoryId, Guid productId)
        {
            InventoryId = inventoryId;
            ProductId = productId;
        }
    }
}