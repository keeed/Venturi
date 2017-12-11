using System;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Xer.Cqrs.QueryStack;

namespace ViewModels.Queries
{
    public class GetProductByIdQuery : IQuery<ProductView>
    {
        public Guid ProductId { get; }

        public GetProductByIdQuery(Guid productId)
        {
            ProductId = productId;
        }
    }

    public class GetProductByIdQueryHandler : IQueryAsyncHandler<GetProductByIdQuery, ProductView>
    {
        private readonly IMongoCollection<ProductView> _productViewCollection;

        public GetProductByIdQueryHandler(IMongoClient mongoClient)
        {
            IMongoDatabase mongoDb = mongoClient.GetDatabase(Constants.MongoDatabaseName);
            _productViewCollection = mongoDb.GetCollection<ProductView>(nameof(ProductView));
        }

        public async Task<ProductView> HandleAsync(GetProductByIdQuery query, CancellationToken cancellationToken = default(CancellationToken))
        {
            var filter = Builders<ProductView>.Filter.Eq(p => p.ProductId, query.ProductId);
            IAsyncCursor<ProductView> result = await _productViewCollection.FindAsync(filter, null, cancellationToken).ConfigureAwait(false);
            return await result.FirstOrDefaultAsync();
        }
    }
}