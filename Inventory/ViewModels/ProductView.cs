using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.DomainEvents;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Xer.Cqrs.EventStack;

namespace ViewModels
{
    public class ProductView
    {
        [BsonId]
        public Guid ProductId { get; set; }
        public Guid InventoryId { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public decimal ProductPrice { get; set; }
        public bool IsProductForSale { get; set; }
    }

    public class ProductViewProjector : IEventAsyncHandler<ProductRegisteredEvent>,
                                        IEventAsyncHandler<ProductUnregisteredEvent>,
                                        IEventAsyncHandler<ProductMarkedForSaleEvent>,
                                        IEventAsyncHandler<ProductMarkedNotForSaleEvent>,
                                        IEventAsyncHandler<ProductRepricedEvent>
    {
        private readonly IMongoCollection<ProductView> _productViewCollection;

        public ProductViewProjector(IMongoClient mongoClient)
        {
            IMongoDatabase mongoDb = mongoClient.GetDatabase(Constants.MongoDatabaseName);
            _productViewCollection = mongoDb.GetCollection<ProductView>(nameof(ProductView));
        }

        public Task HandleAsync(ProductRegisteredEvent @event, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _productViewCollection.InsertOneAsync(new ProductView()
            {
                InventoryId = @event.InventoryId,
                ProductId = @event.ProductId,
                ProductName = @event.ProductName,
                ProductDescription = @event.ProductDescription,
                ProductPrice = @event.ProductPrice,
                IsProductForSale = false // Newly registered products are not for sale. They need to be explicitly marked for sale.
            });
        }

        public Task HandleAsync(ProductUnregisteredEvent @event, CancellationToken cancellationToken = default(CancellationToken))
        {
            var filter = Builders<ProductView>.Filter.Eq(p => p.ProductId, @event.ProductId);
            return _productViewCollection.DeleteOneAsync(filter, cancellationToken);
        }

        public Task HandleAsync(ProductMarkedForSaleEvent @event, CancellationToken cancellationToken = default(CancellationToken))
        {
            var filter = Builders<ProductView>.Filter.Eq(p => p.ProductId, @event.ProductId);
            var update = Builders<ProductView>.Update.Set(p => p.IsProductForSale, true); // For sale.
            return _productViewCollection.FindOneAndUpdateAsync(filter, update, null, cancellationToken);
        }

        public Task HandleAsync(ProductMarkedNotForSaleEvent @event, CancellationToken cancellationToken = default(CancellationToken))
        {
            var filter = Builders<ProductView>.Filter.Eq(p => p.ProductId, @event.ProductId);
            var update = Builders<ProductView>.Update.Set(p => p.IsProductForSale, false); // Not for sale.
            return _productViewCollection.FindOneAndUpdateAsync(filter, update, null, cancellationToken);
        }

        public Task HandleAsync(ProductRepricedEvent @event, CancellationToken cancellationToken = default(CancellationToken))
        {
            var filter = Builders<ProductView>.Filter.Eq(p => p.ProductId, @event.ProductId);
            var update = Builders<ProductView>.Update.Set(p => p.ProductPrice, @event.NewPrice);
            return _productViewCollection.FindOneAndUpdateAsync(filter, update, null, cancellationToken);
        }
    }
}