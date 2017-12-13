using System;
using Xer.Cqrs.EventStack;

namespace Domain.DomainEvents
{
    public class ProductRemovedFromCategoryEvent : IEvent
    {
        public Guid ProductId { get; }
        public string CategoryName { get; }

        public ProductRemovedFromCategoryEvent(Guid productId, string categoryName)
        {
            ProductId = productId;
            CategoryName = categoryName;
        }
    }
}