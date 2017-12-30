using System;
using Xer.Cqrs.EventStack;

namespace Domain.DomainEvents
{
    public class ProductMarkedForSaleEvent : IEvent
    {
        public Guid ProductId { get; }

        public ProductMarkedForSaleEvent(Guid productId)
        {
            ProductId = productId;
        }
    }
}