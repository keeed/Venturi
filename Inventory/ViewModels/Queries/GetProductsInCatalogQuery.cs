using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Xer.Cqrs.QueryStack;

namespace ViewModels.Queries
{
    public class GetProductsInCatalogQuery : IQuery<ProductCatalogViewModel>
    {
        public string CatalogName { get; }

        public GetProductsInCatalogQuery(string catalogName)
        {
            CatalogName = catalogName;
        }
    }

    public class GetProductsInCatalogQueryHandler : IQueryAsyncHandler<GetProductsInCatalogQuery, ProductCatalogViewModel>
    {
        private readonly IMongoCollection<ProductCatalogViewModel> _productCatalogViewCollection;

        public GetProductsInCatalogQueryHandler(IMongoClient mongoClient)
        {
            IMongoDatabase mongoDb = mongoClient.GetDatabase(Constants.MongoDatabaseName);
            _productCatalogViewCollection = mongoDb.GetCollection<ProductCatalogViewModel>(nameof(ProductCatalogViewModel));
        }

        public async Task<ProductCatalogViewModel> HandleAsync(GetProductsInCatalogQuery query, CancellationToken cancellationToken = default(CancellationToken))
        {
            var filter = Builders<ProductCatalogViewModel>.Filter.Eq(pcv => pcv.CatalogName, query.CatalogName);
            IAsyncCursor<ProductCatalogViewModel> findResult = await _productCatalogViewCollection.FindAsync(filter, null, cancellationToken);
            return await findResult.FirstOrDefaultAsync(); 
        }
    }
}