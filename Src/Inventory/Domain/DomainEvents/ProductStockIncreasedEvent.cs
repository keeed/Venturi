using System;
using Xer.Cqrs.EventStack;

namespace Domain.DomainEvents
{
    public class ProductStockIncreasedEvent : IEvent
    {
        public Guid ProductId { get; }
        public int NewStockQuantity { get; }

        public ProductStockIncreasedEvent(Guid productId, int newStockQuantity)
        {
            ProductId = productId;
            NewStockQuantity = newStockQuantity;
        }
    }
}