using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Xer.Cqrs.QueryStack;

namespace ViewModels.Queries
{
    public class GetAllCatalogsQuery : IQuery<ProductCatalogListView>
    {
        
    }

    public class GetAllCatalogsQueryHandler : IQueryAsyncHandler<GetAllCatalogsQuery, ProductCatalogListView>
    {
        private readonly IMongoCollection<ProductCatalogView> _productCatalogViewCollection;

        public GetAllCatalogsQueryHandler(IMongoClient mongoClient)
        {
            IMongoDatabase mongoDb = mongoClient.GetDatabase(Constants.MongoDatabaseName);
            _productCatalogViewCollection = mongoDb.GetCollection<ProductCatalogView>(nameof(ProductCatalogView));
        }

        public async Task<ProductCatalogListView> HandleAsync(GetAllCatalogsQuery query, CancellationToken cancellationToken = default(CancellationToken))
        {
            List<ProductCatalogView> catalogs = await _productCatalogViewCollection.AsQueryable().ToListAsync(cancellationToken);
            return new ProductCatalogListView()
            {
                Catalogs = catalogs
            };
        }
    }
}