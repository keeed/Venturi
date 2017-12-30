using System;
using Xer.Cqrs.EventStack;

namespace Domain.DomainEvents
{
    public class ProductRepricedEvent : IEvent
    {
        public Guid ProductId { get; }
        public decimal NewPrice { get; }

        public ProductRepricedEvent(Guid productId,decimal newAmount)
        {
            ProductId = productId;
            NewPrice = newAmount;
        }
    }
}