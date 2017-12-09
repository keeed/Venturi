using System;

namespace Domain
{
    public class Stock // Value object
    {
        public Stock(ProductId productId, int quantity)
        {
            ProductId = productId;
            Quantity = quantity;
        }

        public ProductId ProductId { get; private set; }
        public int Quantity { get; private set; }

        public Stock IncreaseQuantity(int amountToIncrease)
        {
            return new Stock(ProductId, Quantity + amountToIncrease);
        }

        public Stock DecreaseQuantity(int amountToDecrease)
        {
            int newQuantity = Quantity - amountToDecrease;
            if (newQuantity < 0)
            {
                throw new InvalidOperationException("Cannot decrease stock quantity by an amount greater than the curent quantity.");
            }

            return new Stock(ProductId, newQuantity);
        }
    }
}