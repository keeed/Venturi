using System;
using Xer.Cqrs.EventStack;

namespace Domain.DomainEvents
{
    public class ProductStockDecreasedEvent : IEvent
    {
        public Guid InventoryId { get; }
        public Guid ProductId { get; }
        public int NewStockQuantity { get; }

        public ProductStockDecreasedEvent(Guid inventoryId, Guid productId, int newStockQuantity)
        {
            InventoryId = inventoryId;
            ProductId = productId;
            NewStockQuantity = newStockQuantity;
        }
    }
}