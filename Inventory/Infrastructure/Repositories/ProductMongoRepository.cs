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

        public ProductMongoRepository(IMongoClient mongoClient)
        {
            IMongoDatabase mongoDatabase = mongoClient.GetDatabase(Constants.MongoDatabaseName);
            _productCollection = mongoDatabase.GetCollection<ProductMongoDocument>(nameof(Inventory));
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
            return new Product(productDocId,
                               new InventoryId(productDoc.InventoryId),
                               productDoc.ProductName,
                               productDoc.ProductDescription,
                               new Price(productDoc.Price),
                               new Stock(productDocId, productDoc.StockQuantity),
                               productDoc.Catalogs.Select(cDoc => new Catalog(new InventoryId(cDoc.InventoryId), cDoc.CatalogName)).ToList(), // Translate
                               productDoc.IsForSale);
        }

        public Task SaveAsync(Product product, CancellationToken ct = default(CancellationToken))
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }

            ProductState state = product.GetCurrentState();

            var filter = Builders<ProductMongoDocument>.Filter.Eq(p => p.ProductId, state.ProductId.Value);
            
            var update = Builders<ProductMongoDocument>.Update.Set(p => p.InventoryId, state.InventoryId.Value)
                                                              .Set(p => p.ProductName, state.ProductName)
                                                              .Set(p => p.ProductDescription, state.ProductDescription)
                                                              .Set(p => p.Price, state.Price.Amount)
                                                              .Set(p => p.StockQuantity, state.Stock.Quantity)
                                                              .Set(p => p.Catalogs, state.Catalogs.Select(c => new CatalogMongoDocument()
                                                                                                          {
                                                                                                              InventoryId = c.InventoryId.Value,
                                                                                                              CatalogName = c.Name
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

        private class ProductMongoDocument
        {
            [BsonId]
            public Guid ProductId { get; set; }
            public Guid InventoryId { get; set; }
            public string ProductName { get; set; }
            public string ProductDescription { get; set; }
            public decimal Price { get; set; }
            public int StockQuantity { get; set; }
            public ICollection<CatalogMongoDocument> Catalogs { get; set; } = new List<CatalogMongoDocument>();
            public bool IsForSale { get; set; }
            public bool IsUnregistered { get; set; }
        }

        private class CatalogMongoDocument
        {
            public Guid InventoryId { get; set; }
            public string CatalogName { get; set; }
        }
    }
}