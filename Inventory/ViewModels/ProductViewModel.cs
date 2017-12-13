using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.DomainEvents;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Xer.Cqrs.EventStack;

namespace ViewModels
{
    public class ProductViewModel
    {
        [BsonId]
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public decimal ProductPrice { get; set; }
        public bool IsProductForSale { get; set; }
    }

    public class ProductViewModelProjector : IEventAsyncHandler<ProductRegisteredEvent>,
                                             IEventAsyncHandler<ProductUnregisteredEvent>,
                                             IEventAsyncHandler<ProductMarkedForSaleEvent>,
                                             IEventAsyncHandler<ProductMarkedNotForSaleEvent>,
                                             IEventAsyncHandler<ProductRepricedEvent>
    {
        private readonly IMongoCollection<ProductViewModel> _productViewCollection;

        public ProductViewModelProjector(QueryMongoDatabase mongoDb)
        {
            _productViewCollection = mongoDb.GetCollection<ProductViewModel>(nameof(ProductViewModel));
        }

        public Task HandleAsync(ProductRegisteredEvent @event, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _productViewCollection.InsertOneAsync(new ProductViewModel()
            {
                ProductId = @event.ProductId,
                ProductName = @event.ProductName,
                ProductDescription = @event.ProductDescription,
                ProductPrice = @event.ProductPrice,
                IsProductForSale = false // Newly registered products are not for sale. They need to be explicitly marked for sale.
            });
        }

        public Task HandleAsync(ProductUnregisteredEvent @event, CancellationToken cancellationToken = default(CancellationToken))
        {
            var filter = Builders<ProductViewModel>.Filter.Eq(p => p.ProductId, @event.ProductId);
            return _productViewCollection.DeleteOneAsync(filter, cancellationToken);
        }

        public Task HandleAsync(ProductMarkedForSaleEvent @event, CancellationToken cancellationToken = default(CancellationToken))
        {
            var filter = Builders<ProductViewModel>.Filter.Eq(p => p.ProductId, @event.ProductId);
            var update = Builders<ProductViewModel>.Update.Set(p => p.IsProductForSale, true); // For sale.
            return _productViewCollection.FindOneAndUpdateAsync(filter, update, null, cancellationToken);
        }

        public Task HandleAsync(ProductMarkedNotForSaleEvent @event, CancellationToken cancellationToken = default(CancellationToken))
        {
            var filter = Builders<ProductViewModel>.Filter.Eq(p => p.ProductId, @event.ProductId);
            var update = Builders<ProductViewModel>.Update.Set(p => p.IsProductForSale, false); // Not for sale.
            return _productViewCollection.FindOneAndUpdateAsync(filter, update, null, cancellationToken);
        }

        public Task HandleAsync(ProductRepricedEvent @event, CancellationToken cancellationToken = default(CancellationToken))
        {
            var filter = Builders<ProductViewModel>.Filter.Eq(p => p.ProductId, @event.ProductId);
            var update = Builders<ProductViewModel>.Update.Set(p => p.ProductPrice, @event.NewPrice);
            return _productViewCollection.FindOneAndUpdateAsync(filter, update, null, cancellationToken);
        }
    }
}