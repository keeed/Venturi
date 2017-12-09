using System;

namespace Domain
{
    public class Price
    {
        public decimal Amount { get; private set; }

        public Price(decimal amount)
        {
            if(amount < 0)
            {
                throw new InvalidOperationException("Product price should not be negative.");
            }

            Amount = amount;
        }

        public Price ChangePrice(decimal newAmount)
        {
            if(newAmount < 0)
            {
                throw new InvalidOperationException("Product price should not be negative.");
            }
            
            return new Price(newAmount);
        }
    }
}