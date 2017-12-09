using System;
using Xer.Cqrs.EventStack;

namespace Domain.DomainEvents
{
    public class ProductUnmarkedForSaleEvent : IEvent
    {
        public Guid InventoryId { get; }
        public Guid ProductId { get; }

        public ProductUnmarkedForSaleEvent(Guid InventoryId, Guid productId)
        {
            this.InventoryId = InventoryId;
            ProductId = productId;
        }
    }
}