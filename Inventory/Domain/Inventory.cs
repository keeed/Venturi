using System;
using System.Collections.Generic;
using System.Linq;
using Domain.DomainEvents;
using Xer.Cqrs.EventStack;

namespace Domain
{
    public class Inventory : IEventSource // Aggregate
    {
        // These events will be published when entity is saved using PublishingRepository
        private readonly List<IEvent> _events = new List<IEvent>();
        private readonly List<Product> _products = new List<Product>();
        private readonly List<Catalog> _catalogs = new List<Catalog>();

        public InventoryId Id { get; private set; }
        public WarehouseId WarehouseId { get; private set; }

        public IReadOnlyCollection<Product> Products => _products.AsReadOnly();
        IEnumerable<IEvent> IEventSource.Events => _events.AsReadOnly();
        
        public Inventory(InventoryId inventoryId, WarehouseId warehouseId)
        {
            Id = inventoryId;
            WarehouseId = warehouseId;
        }

        public Inventory(Inventory copy)
        {
            Id = copy.Id;
            WarehouseId = copy.WarehouseId;
            _events = copy._events;
            _products = copy._products;
            _catalogs = copy._catalogs;
        }

        public void RegisterNewProduct(ProductId productId, string productName, string productDescription, decimal productPrice)
        {            
            _products.Add(new Product(productId,
                                      new Stock(productId, 0),
                                      productName,
                                      productDescription,
                                      new Price(productPrice),
                                      false));

            _events.Add(new ProductRegisteredEvent(Id.Value, 
                                                   productId.Value, 
                                                   productName, 
                                                   productDescription,
                                                   productPrice));
        }

        public void UnregisterProduct(ProductId productId)
        {
            _products.RemoveAll(p => p.Id == productId);

            _events.Add(new ProductUnregisteredEvent(Id.Value, productId.Value));
        }

        public void IncreaseProductStock(ProductId productId, int amountToIncrease)
        {
            Product product = GetProductById(productId);
            product.IncreaseProductStock(amountToIncrease);

            _events.Add(new ProductMarkedForSaleEvent(Id.Value, productId.Value));
        }

        public void DecreaseProductStock(ProductId productId, int amountToDecrease)
        {
            Product product = GetProductById(productId);
            product.DecreaseProductStock(amountToDecrease);

            _events.Add(new ProductStockDecreasedEvent(Id.Value, productId.Value, amountToDecrease));
        }

        public void MarkProductForSale(ProductId productId)
        {
            Product product = GetProductById(productId);
            product.MarkForSale();

            _events.Add(new ProductMarkedForSaleEvent(Id.Value, productId.Value));
        }

        public void UnmarkProductForSale(ProductId productId)
        {
            Product product = GetProductById(productId);
            product.UnmarkForSale();

            _events.Add(new ProductUnmarkedForSaleEvent(Id.Value, productId.Value));
        }

        public void RepriceProduct(ProductId productId, decimal newAmount)
        {
            Product product = GetProductById(productId);
            product.Reprice(newAmount);

            _events.Add(new ProductRepricedEvent(Id.Value, productId.Value, newAmount));
        }

        public void CreateNewCatalog(string catalogName)
        {
            _catalogs.Add(new Catalog(Id, catalogName));
            
            _events.Add(new CatalogCreatedEvent(Id.Value, catalogName));
        }

        public void DeleteCatalog(string catalogName)
        {
            _catalogs.RemoveAll(c => c.Name == catalogName);

            _events.Add(new CatalogDeletedEvent(Id.Value, catalogName));
        }

        public void AddProductToCatalog(ProductId productId, string catalogName)
        {
            Product product = GetProductById(productId);
            Catalog catalog = GetCatalogByName(catalogName);
            product.AddProductToCatalog(catalog);

            _events.Add(new ProductAddedToCatalogEvent(Id.Value, productId.Value, catalogName));
        }

        public void RemoveProductFromCatalog(ProductId productId, string catalogName)
        {
            Product product = GetProductById(productId);
            Catalog catalog = GetCatalogByName(catalogName);
            product.RemoveProductFromCatalog(catalog);

            _events.Add(new ProductRemovedFromCatalogEvent(Id.Value, productId.Value, catalogName));
        }

        private Product GetProductById(ProductId productId)
        {
            Product product = _products.FirstOrDefault(p => p.Id == productId);
            if(product == null)
            {
                throw new InvalidOperationException("Product not found in inventory.");
            }
            return product;
        }

        private Catalog GetCatalogByName(string catalogName)
        {
            Catalog catalog = _catalogs.FirstOrDefault(c => c.Name == catalogName);
            if(catalog == null)
            {
                throw new InvalidOperationException("Catalog does not exist in inventory.");
            }
            return catalog;
        }
    }
}