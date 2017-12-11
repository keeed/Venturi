using System;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Xer.Cqrs.QueryStack;

namespace ViewModels.Queries
{
    public class GetProductByIdQuery : IQuery<ProductViewModel>
    {
        public Guid ProductId { get; }

        public GetProductByIdQuery(Guid productId)
        {
            ProductId = productId;
        }
    }

    public class GetProductByIdQueryHandler : IQueryAsyncHandler<GetProductByIdQuery, ProductViewModel>
    {
        private readonly IMongoCollection<ProductViewModel> _productViewCollection;

        public GetProductByIdQueryHandler(IMongoClient mongoClient)
        {
            IMongoDatabase mongoDb = mongoClient.GetDatabase(Constants.MongoDatabaseName);
            _productViewCollection = mongoDb.GetCollection<ProductViewModel>(nameof(ProductViewModel));
        }

        public async Task<ProductViewModel> HandleAsync(GetProductByIdQuery query, CancellationToken cancellationToken = default(CancellationToken))
        {
            var filter = Builders<ProductViewModel>.Filter.Eq(p => p.ProductId, query.ProductId);
            IAsyncCursor<ProductViewModel> result = await _productViewCollection.FindAsync(filter, null, cancellationToken).ConfigureAwait(false);
            return await result.FirstOrDefaultAsync();
        }
    }
}