using System;
using Xer.Cqrs.EventStack;

namespace Domain.DomainEvents
{
    public class ProductRepricedEvent : IEvent
    {
        public Guid InventoryId { get; }
        public Guid ProductId { get; }
        public decimal NewPrice { get; }

        public ProductRepricedEvent(Guid inventoryId, Guid productId, decimal newAmount)
        {
            InventoryId = inventoryId;
            ProductId = productId;
            NewPrice = newAmount;
        }
    }
}