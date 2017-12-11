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
    public class ProductCatalogView
    {
        [BsonId]        
        public string CatalogName { get; set; }
        public ICollection<ProductCatalogViewEntry> ProductsInCatalog { get; set; }

        public class ProductCatalogViewEntry
        {
            public Guid ProductId { get; set; }
        }
    }

    public class ProductCatalogViewProjector : IEventAsyncHandler<CatalogCreatedEvent>,
                                               IEventAsyncHandler<CatalogDeletedEvent>,
                                               IEventAsyncHandler<ProductAddedToCatalogEvent>,
                                               IEventAsyncHandler<ProductRemovedFromCatalogEvent>
    {
        private readonly IMongoCollection<ProductCatalogView> _productCatalogViewCollection;

        public ProductCatalogViewProjector(IMongoClient mongoClient)
        {
            IMongoDatabase mongoDb = mongoClient.GetDatabase(Constants.MongoDatabaseName);
            _productCatalogViewCollection = mongoDb.GetCollection<ProductCatalogView>(nameof(ProductCatalogView));
        }

        public Task HandleAsync(CatalogCreatedEvent @event, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _productCatalogViewCollection.InsertOneAsync(new ProductCatalogView()
            {
                CatalogName = @event.CatalogName,
                ProductsInCatalog = new List<ProductCatalogView.ProductCatalogViewEntry>()
            });
        }

        public Task HandleAsync(CatalogDeletedEvent @event, CancellationToken cancellationToken = default(CancellationToken))
        {
            var filter = Builders<ProductCatalogView>.Filter.Eq(c => c.CatalogName, @event.CatalogName);
            return _productCatalogViewCollection.DeleteOneAsync(, cancellationToken);
        }

        public async Task HandleAsync(ProductAddedToCatalogEvent @event, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Get catalog.
            var filter = Builders<ProductCatalogView>.Filter.Eq(pcv => pcv.CatalogName, @event.CatalogName);
            IAsyncCursor<ProductCatalogView> findResult = await _productCatalogViewCollection.FindAsync(filter, null, cancellationToken);

            // Perform update to catalog.
            ProductCatalogView catalogView = await findResult.FirstOrDefaultAsync(cancellationToken);
            catalogView.ProductsInCatalog.Add(new ProductCatalogView.ProductCatalogViewEntry()
            {
                ProductId = @event.ProductId
            });

            // Send update to database.
            var update = Builders<ProductCatalogView>.Update.Set(pcv => pcv, catalogView);
            await _productCatalogViewCollection.FindOneAndUpdateAsync(filter, update, null, cancellationToken);
        }

        public async Task HandleAsync(ProductRemovedFromCatalogEvent @event, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Get catalog.
            var filter = Builders<ProductCatalogView>.Filter.Eq(c => c.CatalogName, @event.CatalogName);
            IAsyncCursor<ProductCatalogView> findResult = await _productCatalogViewCollection.FindAsync(filter, null, cancellationToken);

            // Perform update to catalog.
            ProductCatalogView catalogView = await findResult.FirstOrDefaultAsync(cancellationToken);
            catalogView.ProductsInCatalog.Remove(new ProductCatalogView.ProductCatalogViewEntry()
            {
                ProductId = @event.ProductId
            });

            // Send update to database.
            var update = Builders<ProductCatalogView>.Update.Set(c => c, catalogView);
            await _productCatalogViewCollection.FindOneAndUpdateAsync(filter, update, null, cancellationToken);
        }
    }
}