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
    public class ProductCatalogViewModel
    {
        [BsonId]        
        public string CatalogName { get; set; }
        public ICollection<ProductCatalogViewEntry> ProductsInCatalog { get; set; }

        public class ProductCatalogViewEntry
        {
            public Guid ProductId { get; set; }
        }
    }

    public class ProductCatalogViewModelProjector : IEventAsyncHandler<CatalogCreatedEvent>,
                                                    IEventAsyncHandler<CatalogDeletedEvent>,
                                                    IEventAsyncHandler<ProductAddedToCatalogEvent>,
                                                    IEventAsyncHandler<ProductRemovedFromCatalogEvent>
    {
        private readonly IMongoCollection<ProductCatalogViewModel> _productCatalogViewCollection;

        public ProductCatalogViewModelProjector(IMongoClient mongoClient)
        {
            IMongoDatabase mongoDb = mongoClient.GetDatabase(Constants.MongoDatabaseName);
            _productCatalogViewCollection = mongoDb.GetCollection<ProductCatalogViewModel>(nameof(ProductCatalogViewModel));
        }

        public Task HandleAsync(CatalogCreatedEvent @event, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _productCatalogViewCollection.InsertOneAsync(new ProductCatalogViewModel()
            {
                CatalogName = @event.CatalogName,
                ProductsInCatalog = new List<ProductCatalogViewModel.ProductCatalogViewEntry>()
            });
        }

        public Task HandleAsync(CatalogDeletedEvent @event, CancellationToken cancellationToken = default(CancellationToken))
        {
            var filter = Builders<ProductCatalogViewModel>.Filter.Eq(c => c.CatalogName, @event.CatalogName);
            return _productCatalogViewCollection.FindOneAndDeleteAsync(filter, null, cancellationToken);
        }

        public async Task HandleAsync(ProductAddedToCatalogEvent @event, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Get catalog.
            var filter = Builders<ProductCatalogViewModel>.Filter.Eq(pcv => pcv.CatalogName, @event.CatalogName);
            IAsyncCursor<ProductCatalogViewModel> findResult = await _productCatalogViewCollection.FindAsync(filter, null, cancellationToken);

            // Perform update to catalog.
            ProductCatalogViewModel catalogView = await findResult.FirstOrDefaultAsync(cancellationToken);
            catalogView.ProductsInCatalog.Add(new ProductCatalogViewModel.ProductCatalogViewEntry()
            {
                ProductId = @event.ProductId
            });

            // Send update to database.
            var update = Builders<ProductCatalogViewModel>.Update.Set(pcv => pcv, catalogView);
            await _productCatalogViewCollection.FindOneAndUpdateAsync(filter, update, null, cancellationToken);
        }

        public async Task HandleAsync(ProductRemovedFromCatalogEvent @event, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Get catalog.
            var filter = Builders<ProductCatalogViewModel>.Filter.Eq(c => c.CatalogName, @event.CatalogName);
            IAsyncCursor<ProductCatalogViewModel> findResult = await _productCatalogViewCollection.FindAsync(filter, null, cancellationToken);

            // Perform update to catalog.
            ProductCatalogViewModel catalogView = await findResult.FirstOrDefaultAsync(cancellationToken);
            catalogView.ProductsInCatalog.Remove(new ProductCatalogViewModel.ProductCatalogViewEntry()
            {
                ProductId = @event.ProductId
            });

            // Send update to database.
            var update = Builders<ProductCatalogViewModel>.Update.Set(c => c, catalogView);
            await _productCatalogViewCollection.FindOneAndUpdateAsync(filter, update, null, cancellationToken);
        }
    }
}