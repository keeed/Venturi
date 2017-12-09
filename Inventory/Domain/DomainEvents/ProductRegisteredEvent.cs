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

        public ProductRegisteredEvent(Guid inventoryId, 
                                      Guid productId, 
                                      string productName, 
                                      string productDescription, 
                                      decimal productPrice)
        {
            InventoryId = inventoryId;
            ProductId = productId;
            ProductName = productName;
            ProductDescription = productDescription;
            ProductPrice = productPrice;
        }
    }
}