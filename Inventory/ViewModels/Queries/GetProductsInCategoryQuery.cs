using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Xer.Cqrs.QueryStack;

namespace ViewModels.Queries
{
    public class GetProductsInCategoryQuery : IQuery<ProductCategoryViewModel>
    {
        public string CategoryName { get; }

        public GetProductsInCategoryQuery(string catalogName)
        {
            CategoryName = catalogName;
        }
    }

    public class GetProductsInCatalogQueryHandler : IQueryAsyncHandler<GetProductsInCategoryQuery, ProductCategoryViewModel>
    {
        private readonly IMongoCollection<ProductCategoryViewModel> _productCatalogViewCollection;

        public GetProductsInCatalogQueryHandler(QueryMongoDatabase mongoDb)
        {
            _productCatalogViewCollection = mongoDb.GetCollection<ProductCategoryViewModel>(nameof(ProductCategoryViewModel));
        }

        public async Task<ProductCategoryViewModel> HandleAsync(GetProductsInCategoryQuery query, CancellationToken cancellationToken = default(CancellationToken))
        {
            var filter = Builders<ProductCategoryViewModel>.Filter.Eq(pcv => pcv.CategoryName, query.CategoryName);
            var findResult = await _productCatalogViewCollection.FindAsync(filter, null, cancellationToken);
            return await findResult.FirstOrDefaultAsync(cancellationToken); 
        }
    }
}