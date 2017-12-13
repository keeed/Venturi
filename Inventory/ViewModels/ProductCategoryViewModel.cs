using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.DomainEvents;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Xer.Cqrs.EventStack;

namespace ViewModels
{
    public class ProductCategoryViewModel
    {
        [BsonId]        
        public string CategoryName { get; set; }
        public List<ProductEntry> ProductsInCategory { get; set; }

        public class ProductEntry
        {
            public Guid ProductId { get; set; }
            public string ProductName { get; set; }
        }

        public void AddProduct(Guid productId, string productName)
        {
            if(productId == Guid.Empty)
            {
                throw new ArgumentException("Invalid product ID.", nameof(productName));
            }

            if (string.IsNullOrWhiteSpace(productName))
            {
                throw new ArgumentException("Invalid product name.", nameof(productName));
            }

            ProductsInCategory.Add(new ProductEntry()
            {
                ProductId = productId,
                ProductName = productName
            });
        }

        public void RemoveProduct(Guid productId)
        {
            ProductsInCategory.RemoveAll(p => p.ProductId == productId);
        }
    }

    public class ProductCategoryViewModelProjector : IEventAsyncHandler<ProductAddedToCategoryEvent>,
                                                     IEventAsyncHandler<ProductRemovedFromCategoryEvent>
                                                    // IEventAsyncHandler<CatalogCreatedEvent>,
                                                    // IEventAsyncHandler<CatalogDeletedEvent>
    {
        private readonly IMongoCollection<ProductCategoryViewModel> _categoryViewCollection;
        private readonly IMongoCollection<ProductViewModel> _productViewCollection;

        public ProductCategoryViewModelProjector(QueryMongoDatabase mongoDb)
        {
            _categoryViewCollection = mongoDb.GetCollection<ProductCategoryViewModel>(nameof(ProductCategoryViewModel));
            _productViewCollection = mongoDb.GetCollection<ProductViewModel>(nameof(ProductViewModel));
        }

        public async Task HandleAsync(ProductAddedToCategoryEvent @event, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Get product task.
            var productFilter = Builders<ProductViewModel>.Filter.Eq(pcv => pcv.ProductId, @event.ProductId);
            var findProductTask = _productViewCollection.FindAsync(productFilter, null, cancellationToken);
            
            // Get category task.
            var categoryFilter = Builders<ProductCategoryViewModel>.Filter.Eq(pcv => pcv.CategoryName, @event.CategoryName);
            var findCategoryTask = _categoryViewCollection.FindAsync(categoryFilter, null, cancellationToken);

            // Retrieve product.
            var findProductResult = await findProductTask;
            ProductViewModel productView = await findProductResult.FirstOrDefaultAsync(cancellationToken);

            if(productView == null)
            {
                // Product not found. Nothing to do. Return.
                return;
            }

            // Retrieve category view.
            var findCategoryResult = await findCategoryTask;
            ProductCategoryViewModel categoryView = await findCategoryResult.FirstOrDefaultAsync(cancellationToken);

            if(categoryView == null)
            {
                categoryView = new ProductCategoryViewModel()
                {
                    CategoryName = @event.CategoryName,
                    ProductsInCategory = new List<ProductCategoryViewModel.ProductEntry>()
                };
            }

            // Perform update to category view.
            categoryView.AddProduct(productView.ProductId, productView.ProductName);

            // Send update to database.
            var update = Builders<ProductCategoryViewModel>.Update.Set(c => c.CategoryName, categoryView.CategoryName)
                                                                  .Set(c => c.ProductsInCategory, categoryView.ProductsInCategory);

            await _categoryViewCollection.FindOneAndUpdateAsync(categoryFilter, 
                                                                update, 
                                                                new FindOneAndUpdateOptions<ProductCategoryViewModel>()
                                                                {
                                                                    IsUpsert = true
                                                                }, 
                                                                cancellationToken);
        }

        public async Task HandleAsync(ProductRemovedFromCategoryEvent @event, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Get catalog.
            var filter = Builders<ProductCategoryViewModel>.Filter.Eq(c => c.CategoryName, @event.CategoryName);
            var findResult = await _categoryViewCollection.FindAsync(filter, null, cancellationToken);

            // Perform update to catalog.
            ProductCategoryViewModel categoryView = await findResult.FirstOrDefaultAsync(cancellationToken);
            if(categoryView == null)
            {
                // Do nothing.
                return;
            }

            categoryView.RemoveProduct(@event.ProductId);

            // Send update to database.
            var update = Builders<ProductCategoryViewModel>.Update.Set(c => c.CategoryName, categoryView.CategoryName)
                                                                  .Set(c => c.ProductsInCategory, categoryView.ProductsInCategory);
                                                                  
            await _categoryViewCollection.FindOneAndUpdateAsync(filter, update, null, cancellationToken);
        }
    }
}