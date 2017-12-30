using System;
using Xer.Cqrs.EventStack;

namespace Domain.DomainEvents
{
    public class ProductUnregisteredEvent : IEvent
    {
        public Guid ProductId { get; }

        public ProductUnregisteredEvent(Guid productId)
        {
            ProductId = productId;
        }
    }
}