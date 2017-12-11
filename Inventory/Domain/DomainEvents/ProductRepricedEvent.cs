using System;
using Xer.Cqrs.EventStack;

namespace Domain.DomainEvents
{
    public class ProductRepricedEvent : IEvent
    {
        public Guid ProductId { get; }
        public Guid InventoryId { get; }
        public decimal NewPrice { get; }

        public ProductRepricedEvent(Guid productId, Guid inventoryId, decimal newAmount)
        {
            ProductId = productId;
            InventoryId = inventoryId;
            NewPrice = newAmount;
        }
    }
}