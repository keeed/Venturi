using System;
using Xer.Cqrs.EventStack;

namespace Domain.DomainEvents
{
    public class ProductRegisteredEvent : IEvent
    {
        public Guid InventoryId { get; }
        public Guid ProductId { get; }
        public string ProductName { get; }
        public string ProductDescription { get; }
        public decimal ProductPrice { get; }

        public ProductRegisteredEvent(Guid productId, 
                                      Guid inventoryId, 
                                      string productName, 
                                      string productDescription, 
                                      decimal productPrice)
        {
            ProductId = productId;
            InventoryId = inventoryId;
            ProductName = productName;
            ProductDescription = productDescription;
            ProductPrice = productPrice;
        }
    }
}