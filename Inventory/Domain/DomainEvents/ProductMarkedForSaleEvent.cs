using System;
using Xer.Cqrs.EventStack;

namespace Domain.DomainEvents
{
    public class ProductMarkedForSaleEvent : IEvent
    {
        public Guid InventoryId { get; }
        public Guid ProductId { get; }

        public ProductMarkedForSaleEvent(Guid inventoryId, Guid productId)
        {
            InventoryId = inventoryId;
            ProductId = productId;
        }
    }
}