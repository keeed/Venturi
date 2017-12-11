using System;
using Xer.Cqrs.EventStack;

namespace Domain.DomainEvents
{
    public class ProductStockIncreasedEvent : IEvent
    {
        public Guid ProductId { get; }
        public Guid InventoryId { get; }
        public int NewStockQuantity { get; }

        public ProductStockIncreasedEvent(Guid productId, Guid inventoryId, int newStockQuantity)
        {
            ProductId = productId;
            InventoryId = inventoryId;
            NewStockQuantity = newStockQuantity;
        }
    }
}