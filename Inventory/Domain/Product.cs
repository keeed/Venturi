using System;
using System.Collections.Generic;
using System.Linq;
using Domain.DomainEvents;
using Xer.Cqrs.EventStack;

namespace Domain
{
    public class Product : IEventSource, IStateContainer<ProductState> // Aggregate
    {
        // These events will be published when entity is saved using PublishingRepository
        private readonly List<IEvent> _events = new List<IEvent>();
        private readonly List<Catalog> _catalogs = new List<Catalog>();
        private readonly ProductId _productId;
        private readonly InventoryId _inventoryId;
        private Stock _stock;
        private string _name;
        private string _description;
        private Price _price;
        private bool _isForSale;
        private bool _isUnregistered;

        public ProductId Id => _productId;

        #region IEventSource Implementation

        IEnumerable<IEvent> IEventSource.GetEvents() => _events.AsReadOnly();
        void IEventSource.ClearEvents() => _events.Clear();

        #endregion IEventSource Implementation

        #region IStateContainer Implementation

        public ProductState GetCurrentState()
        {
            return new ProductState(_productId,
                                    _inventoryId,
                                    _name,
                                    _description,
                                    _price,
                                    _stock,
                                    _catalogs,
                                    _isForSale,
                                    _isUnregistered);
        }

        #endregion IStateContainer Implementation

        public Product(ProductId productId,
                       InventoryId inventoryId,
                       string productName,
                       string productDescription,
                       Price price,
                       Stock stock,
                       IEnumerable<Catalog> catalogs,
                       bool isForSale)
            : this(productId,
                   inventoryId,
                   productName,
                   productDescription,
                   price,
                   stock,
                   catalogs,
                   isForSale, 
                   false) // Not unregisterd
        {
        }

        public Product(ProductId productId,
                       InventoryId inventoryId,
                       string productName,
                       string productDescription,
                       Price price,
                       Stock stock,
                       IEnumerable<Catalog> catalogs,
                       bool isForSale,
                       bool isUnregistered)
        {
            if (string.IsNullOrWhiteSpace(productName))
            {
                throw new ArgumentException("Product name is required.", nameof(productName));
            }

            if (catalogs == null)
            {
                throw new ArgumentNullException(nameof(catalogs));
            }

            _productId = productId ?? throw new ArgumentNullException(nameof(productId));
            _inventoryId = inventoryId;
            _name = productName;
            _description = productDescription;
            _price = price ?? throw new ArgumentNullException(nameof(price));
            _stock = stock ?? throw new ArgumentNullException(nameof(stock));
            _catalogs = catalogs.ToList();
            _isForSale = isForSale;
            _isUnregistered = isUnregistered;
        }

        public void IncreaseProductStock(int amountToIncrease)
        {
            _stock = _stock.IncreaseQuantity(amountToIncrease);

            _events.Add(new ProductStockIncreasedEvent(_productId.Value, _inventoryId.Value, _stock.Quantity));
        }

        public void DecreaseProductStock(int amountToDecrease)
        {
            _stock = _stock.DecreaseQuantity(amountToDecrease);

            _events.Add(new ProductStockDecreasedEvent(_productId.Value, _inventoryId.Value, _stock.Quantity));
        }

        public void MarkForSale()
        {
            _isForSale = true;
            _events.Add(new ProductMarkedForSaleEvent(_productId.Value, _inventoryId.Value));
        }

        public void MarkNotForSale()
        {
            _isForSale = false;
            _events.Add(new ProductMarkedNotForSaleEvent(_productId.Value, _inventoryId.Value));
        }

        public void Reprice(decimal newAmount)
        {
            _price = _price.ChangePrice(newAmount);

            _events.Add(new ProductRepricedEvent(_productId.Value, _inventoryId.Value, newAmount));
        }

        public void AddProductToCatalog(Catalog catalog)
        {
            if (catalog == null)
            {
                throw new ArgumentNullException(nameof(catalog));
            }

            _catalogs.Add(catalog);
        }

        public void RemoveFromCatalog(Catalog catalog)
        {
            if (catalog == null)
            {
                throw new ArgumentNullException(nameof(catalog));
            }

            _catalogs.RemoveAll(c => c.InventoryId == catalog.InventoryId &&
                                     c.Name == catalog.Name);
        }

        public void MarkAsUnregistered()
        {
            _isUnregistered = true; // Unregistered products could be manually cleaned up from DB or could be deleted by a background process.
        }
    }

    #region Product State

    public class ProductState
    {
        public ProductState(ProductId productId, 
                            InventoryId inventoryId, 
                            string productName,
                            string productDescription, 
                            Price price,
                            Stock stock, 
                            IEnumerable<Catalog> catalogs,
                            bool isForSale, 
                            bool isUnregistered)
        {
            ProductId = productId;
            InventoryId = inventoryId;
            ProductName = productName;
            ProductDescription = productDescription;
            Price = price;
            Stock = stock;
            Catalogs = catalogs.ToList().AsReadOnly();
            IsForSale = isForSale;
            IsUnregistered = isUnregistered;
        }

        public ProductId ProductId { get; }
        public InventoryId InventoryId { get; }
        public string ProductName { get; }
        public string ProductDescription { get; }
        public Price Price { get; }
        public Stock Stock { get; }
        public IReadOnlyCollection<Catalog> Catalogs { get; }
        public bool IsForSale { get; }
        public bool IsUnregistered { get; }
    }

    #endregion Product State
}