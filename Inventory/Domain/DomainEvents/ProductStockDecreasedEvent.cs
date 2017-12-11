using System;
using Xer.Cqrs.EventStack;

namespace Domain.DomainEvents
{
    public class ProductStockDecreasedEvent : IEvent
    {
        public Guid ProductId { get; }
        public Guid InventoryId { get; }
        public int NewStockQuantity { get; }

        public ProductStockDecreasedEvent(Guid productId, Guid inventoryId, int newStockQuantity)
        {
            ProductId = productId;
            InventoryId = inventoryId;
            NewStockQuantity = newStockQuantity;
        }
    }
}