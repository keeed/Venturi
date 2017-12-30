using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Xer.Cqrs.QueryStack;

namespace ViewModels.Queries
{
    public class QueryProductCategoryView : IQuery<ProductCategoryViewModel>
    {
        public string CategoryName { get; }

        public QueryProductCategoryView(string catalogName)
        {
            CategoryName = catalogName;
        }
    }

    public class QueryProductCategoryViewHandler : IQueryAsyncHandler<QueryProductCategoryView, ProductCategoryViewModel>
    {
        private readonly IMongoCollection<ProductCategoryViewModel> _productCatalogViewCollection;

        public QueryProductCategoryViewHandler(QueryMongoDatabase mongoDb)
        {
            _productCatalogViewCollection = mongoDb.GetCollection<ProductCategoryViewModel>(nameof(ProductCategoryViewModel));
        }

        public async Task<ProductCategoryViewModel> HandleAsync(QueryProductCategoryView query, CancellationToken cancellationToken = default(CancellationToken))
        {
            var filter = Builders<ProductCategoryViewModel>.Filter.Eq(pcv => pcv.CategoryName, query.CategoryName);
            var findResult = await _productCatalogViewCollection.FindAsync(filter, null, cancellationToken);
            return await findResult.FirstOrDefaultAsync(cancellationToken); 
        }
    }
}