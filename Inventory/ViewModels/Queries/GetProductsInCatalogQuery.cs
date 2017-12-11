using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Xer.Cqrs.QueryStack;

namespace ViewModels.Queries
{
    public class GetProductsInCatalogQuery : IQuery<ProductCatalogView>
    {
        public string CatalogName { get; }

        public GetProductsInCatalogQuery(string catalogName)
        {
            CatalogName = catalogName;
        }
    }

    public class GetProductsInCatalogQueryHandler : IQueryAsyncHandler<GetProductsInCatalogQuery, ProductCatalogView>
    {
        private readonly IMongoCollection<ProductCatalogView> _productCatalogViewCollection;

        public GetProductsInCatalogQueryHandler(IMongoClient mongoClient)
        {
            IMongoDatabase mongoDb = mongoClient.GetDatabase(Constants.MongoDatabaseName);
            _productCatalogViewCollection = mongoDb.GetCollection<ProductCatalogView>(nameof(ProductCatalogView));
        }

        public async Task<ProductCatalogView> HandleAsync(GetProductsInCatalogQuery query, CancellationToken cancellationToken = default(CancellationToken))
        {
            var filter = Builders<ProductCatalogView>.Filter.Eq(pcv => pcv.CatalogName, query.CatalogName);
            IAsyncCursor<ProductCatalogView> findResult = await _productCatalogViewCollection.FindAsync(filter, null, cancellationToken);
            return await findResult.FirstOrDefaultAsync(); 
        }
    }
}