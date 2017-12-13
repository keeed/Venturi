using System;
using Xer.Cqrs.EventStack;

namespace Domain.DomainEvents
{
    public class ProductAddedToCategoryEvent : IEvent
    {
        public Guid ProductId { get; }
        public string CategoryName { get; }

        public ProductAddedToCategoryEvent(Guid productId, string categoryName)
        {
            ProductId = productId;
            CategoryName = categoryName;
        }
    }
}