using System;
using Xer.Cqrs.EventStack;

namespace Domain.DomainEvents
{
    public class ProductMarkedNotForSaleEvent : IEvent
    {
        public Guid ProductId { get; }
        public Guid InventoryId { get; }

        public ProductMarkedNotForSaleEvent(Guid productId, Guid inventoryId)
        {
            ProductId = productId;
            InventoryId = inventoryId;
        }
    }
}