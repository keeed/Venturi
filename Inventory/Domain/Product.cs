using System;
using System.Collections.Generic;

namespace Domain
{
    public class Product // Entity
    {
        private readonly List<Catalog> _catalogs = new List<Catalog>();
        
        public ProductId Id { get; private set; }
        public Stock Stock { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public Price Price { get; private set; }
        public bool IsForSale { get; private set; }
        public IReadOnlyCollection<Catalog> Catalogs => _catalogs.AsReadOnly();

        public Product(ProductId productId,
                       Stock stock,
                       string productName,
                       string productDescription,
                       Price productPrice,
                       bool isForSale)
        {
            if (string.IsNullOrWhiteSpace(productName))
            {
                throw new ArgumentException("Product name is required.", nameof(productName));
            }

            Id = productId ?? throw new ArgumentNullException(nameof(productId));
            Stock = stock ?? throw new ArgumentNullException(nameof(stock));
            Name = productName;
            Description = productDescription;
            Price = productPrice ?? throw new ArgumentNullException(nameof(productPrice));
            IsForSale = isForSale;
        }

        public void IncreaseProductStock(int amountToIncrease)
        {
            Stock = Stock.IncreaseQuantity(amountToIncrease);
        }

        public void DecreaseProductStock(int amountToDecrease)
        {
            Stock = Stock.DecreaseQuantity(amountToDecrease);
        }

        public void MarkForSale()
        {
            IsForSale = true;
        }

        public void UnmarkForSale()
        {
            IsForSale = false;
        }

        public void Reprice(decimal newAmount)
        {
            Price = Price.ChangePrice(newAmount);
        }

        public void AddProductToCatalog(Catalog catalog)
        {
            if (catalog == null)
            {
                throw new ArgumentNullException(nameof(catalog));
            }

            _catalogs.Add(catalog);
        }

        public void RemoveProductFromCatalog(Catalog catalog)
        {
            if (catalog == null)
            {
                throw new ArgumentNullException(nameof(catalog));
            }

            _catalogs.RemoveAll(c => c.InventoryId == catalog.InventoryId &&
                                     c.Name == catalog.Name);
        }
    }
}