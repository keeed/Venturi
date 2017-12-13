using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain;
using Domain.Repositories;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace Infrastructure.Repositories
{
    public class ProductMongoRepository : IProductRepository
    {
        private readonly IMongoCollection<ProductMongoDocument> _productCollection;

        public ProductMongoRepository(InventoryMongoDatabase mongoDb)
        {
            _productCollection = mongoDb.GetCollection<ProductMongoDocument>(nameof(Product));
        }

        public async Task<Product> GetByIdAsync(ProductId productId, CancellationToken ct = default(CancellationToken))
        {
            if (productId == null)
            {
                throw new ArgumentNullException(nameof(productId));
            }

            // Get product by ID.
            var filter = Builders<ProductMongoDocument>.Filter.Eq(p => p.ProductId, productId.Value);
            IAsyncCursor<ProductMongoDocument> result = await _productCollection.FindAsync(filter, null, ct);
            ProductMongoDocument productDoc = await result.FirstOrDefaultAsync(ct);

            if (productDoc == null || productDoc.IsUnregistered)
            {
                // Do not return unregistered product.
                return null;
            }

            // Translate to domain.
            ProductId productDocId = new ProductId(productDoc.ProductId);
            return new Product(new ProductState(productDocId,
                                                productDoc.ProductName,
                                                productDoc.ProductDescription,
                                                new Price(productDoc.Price.Amount, productDoc.Price.Currency),
                                                new Stock(productDocId, productDoc.Stock.Quantity),
                                                productDoc.Categories.Select(cDoc => new ProductCategory(cDoc.CategoryName)).ToList(), // Translate
                                                productDoc.IsForSale,
                                                productDoc.IsUnregistered));
        }

        public Task SaveAsync(Product product, CancellationToken ct = default(CancellationToken))
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }

            ProductState state = product.GetCurrentState();

            var filter = Builders<ProductMongoDocument>.Filter.Eq(p => p.ProductId, state.ProductId.Value);
            
            var update = Builders<ProductMongoDocument>.Update.Set(p => p.ProductName, state.ProductName)
                                                              .Set(p => p.ProductDescription, state.ProductDescription)
                                                              .Set(p => p.Price.Amount, state.Price.Amount)
                                                              .Set(p => p.Price.Currency, state.Price.Currency)
                                                              .Set(p => p.Stock.Quantity, state.Stock.Quantity)
                                                              .Set(p => p.Categories, state.Categories.Select(c => new CategoryMongoDocument()
                                                                                                              {
                                                                                                                  CategoryName = c.Name
                                                                                                              }).ToList())
                                                              .Set(p => p.IsForSale, state.IsForSale)
                                                              .Set(p => p.IsUnregistered, state.IsUnregistered);

            return _productCollection.FindOneAndUpdateAsync(filter, 
                                                            update,
                                                            new FindOneAndUpdateOptions<ProductMongoDocument, ProductMongoDocument>()
                                                            {
                                                                IsUpsert = true
                                                            },
                                                            ct);
        }

        public Task DeleteByIdAsync(ProductId productId, CancellationToken ct = default(CancellationToken))
        {
            var filter = Builders<ProductMongoDocument>.Filter.Eq(p => p.ProductId, productId.Value);
            return _productCollection.FindOneAndDeleteAsync(filter, null, ct);
        }

        private class ProductMongoDocument
        {
            [BsonId]
            public Guid ProductId { get; set; }
            public string ProductName { get; set; }
            public string ProductDescription { get; set; }
            public PriceMongoDocument Price { get; set; }
            public StockMongoDocument Stock { get; set; }
            public ICollection<CategoryMongoDocument> Categories { get; set; } = new List<CategoryMongoDocument>();
            public bool IsForSale { get; set; }
            public bool IsUnregistered { get; set; }
        }

        private class PriceMongoDocument
        {
            public decimal Amount { get; set; }
            public string Currency { get; set; }
        }

        private class StockMongoDocument
        {
            public int Quantity { get; set; }
        }

        private class CategoryMongoDocument
        {
            public string CategoryName { get; set; }
        }
    }
}