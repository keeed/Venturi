using System;

namespace Domain
{
    public class Price
    {
        public decimal Amount { get; }
        public string Currency { get; }

        public Price(decimal amount, string currency)
        {
            if (amount < 0)
            {
                throw new InvalidOperationException("Product price should not be negative.");
            }

            Amount = amount;

            if (string.IsNullOrWhiteSpace(currency))
            {
                throw new ArgumentException("Currency is required.", nameof(currency));
            }
            
            Currency = currency;
        }

        public Price ChangePrice(decimal newAmount)
        {
            if (newAmount < 0)
            {
                throw new InvalidOperationException("Product price should not be negative.");
            }

            return new Price(newAmount, Currency);
        }
    }
}