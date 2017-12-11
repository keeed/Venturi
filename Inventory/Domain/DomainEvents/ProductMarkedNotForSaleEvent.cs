using System;
using Xer.Cqrs.EventStack;

namespace Domain.DomainEvents
{
    public class ProductMarkedNotForSaleEvent : IEvent
    {
        public Guid InventoryId { get; }
        public Guid ProductId { get; }

        public ProductMarkedNotForSaleEvent(Guid InventoryId, Guid productId)
        {
            this.InventoryId = InventoryId;
            ProductId = productId;
        }
    }
}