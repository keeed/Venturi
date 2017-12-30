using System;
using Xer.Cqrs.EventStack;

namespace Domain.DomainEvents
{
    public class ProductRegisteredEvent : IEvent
    {
        public Guid ProductId { get; }
        public string ProductName { get; }
        public string ProductDescription { get; }
        public decimal ProductPrice { get; }
        public string Currency { get; }

        public ProductRegisteredEvent(Guid productId, 
                                      string productName, 
                                      string productDescription, 
                                      decimal productPrice, 
                                      string currency)
        {
            ProductId = productId;
            ProductName = productName;
            ProductDescription = productDescription;
            ProductPrice = productPrice;
            Currency = currency;
        }
    }
}