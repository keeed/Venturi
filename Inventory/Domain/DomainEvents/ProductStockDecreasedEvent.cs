using System;
using Xer.Cqrs.EventStack;

namespace Domain.DomainEvents
{
    public class ProductStockDecreasedEvent : IEvent
    {
        public Guid ProductId { get; }
        public int NewStockQuantity { get; }

        public ProductStockDecreasedEvent(Guid productId, int newStockQuantity)
        {
            ProductId = productId;
            NewStockQuantity = newStockQuantity;
        }
    }
}