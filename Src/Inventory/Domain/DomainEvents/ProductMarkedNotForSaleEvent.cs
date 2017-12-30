using System;
using Xer.Cqrs.EventStack;

namespace Domain.DomainEvents
{
    public class ProductMarkedNotForSaleEvent : IEvent
    {
        public Guid ProductId { get; }

        public ProductMarkedNotForSaleEvent(Guid productId)
        {
            ProductId = productId;
        }
    }
}