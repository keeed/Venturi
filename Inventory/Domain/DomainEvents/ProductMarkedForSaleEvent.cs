using System;
using Xer.Cqrs.EventStack;

namespace Domain.DomainEvents
{
    public class ProductMarkedForSaleEvent : IEvent
    {
        public Guid ProductId { get; }
        public Guid InventoryId { get; }

        public ProductMarkedForSaleEvent(Guid productId, Guid inventoryId)
        {
            ProductId = productId;
            InventoryId = inventoryId;
        }
    }
}