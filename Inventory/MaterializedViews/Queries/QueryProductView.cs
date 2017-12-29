using System;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Xer.Cqrs.QueryStack;

namespace ViewModels.Queries
{
    public class QueryProductView : IQuery<ProductViewModel>
    {
        public Guid ProductId { get; }

        public QueryProductView(Guid productId)
        {
            ProductId = productId;
        }
    }

    public class QueryProductViewHandler : IQueryAsyncHandler<QueryProductView, ProductViewModel>
    {
        private readonly IMongoCollection<ProductViewModel> _productViewCollection;

        public QueryProductViewHandler(QueryMongoDatabase mongoDb)
        {
            _productViewCollection = mongoDb.GetCollection<ProductViewModel>(nameof(ProductViewModel));
        }

        public async Task<ProductViewModel> HandleAsync(QueryProductView query, CancellationToken cancellationToken = default(CancellationToken))
        {
            var filter = Builders<ProductViewModel>.Filter.Eq(p => p.ProductId, query.ProductId);
            var findResult = await _productViewCollection.FindAsync(filter, null, cancellationToken).ConfigureAwait(false);
            return await findResult.FirstOrDefaultAsync(cancellationToken);
        }
    }
}