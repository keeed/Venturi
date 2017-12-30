using System;
using System.Collections.Generic;
using System.Linq;
using Domain.DomainEvents;
using Xer.Cqrs.EventStack;

namespace Domain
{
    public class Product : IEventOriginator, IStateContainer<ProductState> // Aggregate
    {
        // These events will be published when entity is saved using PublishingRepository
        private readonly List<IEvent> _events = new List<IEvent>();
        private readonly List<ProductCategory> _categories = new List<ProductCategory>();
        private readonly ProductId _productId;
        private Stock _stock;
        private string _name;
        private string _description;
        private Price _price;
        private bool _isForSale;
        private bool _isUnregistered;

        public ProductId Id => _productId;

        public Product(ProductId productId,
                       string productName,
                       string productDescription,
                       Price price,
                       Stock stock,
                       IEnumerable<ProductCategory> categories,
                       bool isForSale)
            : this(productId,
                   productName,
                   productDescription,
                   price,
                   stock,
                   categories,
                   isForSale, 
                   false) // Not unregistered
        {
            _events.Add(new ProductRegisteredEvent(productId.Value,
                                                   productName,
                                                   productDescription,
                                                   price.Amount, 
                                                   price.Currency));
        }

        public Product(ProductState state)
            : this(state.ProductId,
                   state.ProductName,
                   state.ProductDescription,
                   state.Price,
                   state.Stock,
                   state.Categories,
                   state.IsForSale, 
                   state.IsUnregistered)
        {
            // Do not add event when using state. 
            // It is assumed that when state is used, product is being loaded from DB.
        }

        private Product(ProductId productId,
                       string productName,
                       string productDescription,
                       Price price,
                       Stock stock,
                       IEnumerable<ProductCategory> categories,
                       bool isForSale,
                       bool isUnregistered)
        {
            if (string.IsNullOrWhiteSpace(productName))
            {
                throw new ArgumentException("Product name is required.", nameof(productName));
            }

            if (categories == null)
            {
                throw new ArgumentNullException(nameof(categories));
            }

            _productId = productId ?? throw new ArgumentNullException(nameof(productId));
            _name = productName;
            _description = productDescription;
            _price = price ?? throw new ArgumentNullException(nameof(price));
            _stock = stock ?? throw new ArgumentNullException(nameof(stock));
            _categories = categories.ToList();
            _isForSale = isForSale;
            _isUnregistered = isUnregistered;
        }

        #region IEventSource Implementation

        IEnumerable<IEvent> IEventOriginator.GetEvents() => _events.AsReadOnly();
        void IEventOriginator.ClearEvents() => _events.Clear();

        #endregion IEventSource Implementation

        #region IStateContainer Implementation

        public ProductState GetCurrentState()
        {
            return new ProductState(_productId,
                                    _name,
                                    _description,
                                    _price,
                                    _stock,
                                    _categories,
                                    _isForSale,
                                    _isUnregistered);
        }

        #endregion IStateContainer Implementation

        #region Methods
        
        public void IncreaseProductStock(int amountToIncrease)
        {
            _stock = _stock.IncreaseQuantity(amountToIncrease);

            _events.Add(new ProductStockIncreasedEvent(_productId.Value, _stock.Quantity));
        }

        public void DecreaseProductStock(int amountToDecrease)
        {
            _stock = _stock.DecreaseQuantity(amountToDecrease);

            _events.Add(new ProductStockDecreasedEvent(_productId.Value, _stock.Quantity));
        }

        public void MarkForSale()
        {
            _isForSale = true;
            _events.Add(new ProductMarkedForSaleEvent(_productId.Value));
        }

        public void MarkNotForSale()
        {
            _isForSale = false;
            _events.Add(new ProductMarkedNotForSaleEvent(_productId.Value));
        }

        public void Reprice(decimal newAmount)
        {
            _price = _price.ChangePrice(newAmount);

            _events.Add(new ProductRepricedEvent(_productId.Value, newAmount));
        }

        public void AddProductToCategory(ProductCategory category)
        {
            if (category == null)
            {
                throw new ArgumentNullException(nameof(category));
            }

            _categories.Add(category);
            _events.Add(new ProductAddedToCategoryEvent(_productId.Value, category.Name));
        }

        public void RemoveFromCategory(ProductCategory category)
        {
            if (category == null)
            {
                throw new ArgumentNullException(nameof(category));
            }

            _categories.RemoveAll(c => c.Name == category.Name);
            _events.Add(new ProductRemovedFromCategoryEvent(_productId.Value, category.Name));
        }

        public void Unregister()
        {
            _isUnregistered = true; // Unregistered products could be manually cleaned up from DB or could be deleted by a background process.
            _events.Add(new ProductUnregisteredEvent(Id.Value));
        }

        public bool HasQuantityInStock(int quantity)
        {
            return _stock.Quantity >= quantity;
        }

        #endregion Methods
    }

    #region Product State

    public class ProductState
    {
        public ProductState(ProductId productId,
                            string productName,
                            string productDescription, 
                            Price price,
                            Stock stock, 
                            IEnumerable<ProductCategory> catalogs,
                            bool isForSale, 
                            bool isUnregistered)
        {
            ProductId = productId;
            ProductName = productName;
            ProductDescription = productDescription;
            Price = price;
            Stock = stock;
            Categories = catalogs.ToList().AsReadOnly();
            IsForSale = isForSale;
            IsUnregistered = isUnregistered;
        }

        public ProductId ProductId { get; }
        public string ProductName { get; }
        public string ProductDescription { get; }
        public Price Price { get; }
        public Stock Stock { get; }
        public IReadOnlyCollection<ProductCategory> Categories { get; }
        public bool IsForSale { get; }
        public bool IsUnregistered { get; }
    }

    #endregion Product State
}