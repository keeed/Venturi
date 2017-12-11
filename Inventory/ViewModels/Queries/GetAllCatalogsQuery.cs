using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Xer.Cqrs.QueryStack;

namespace ViewModels.Queries
{
    public class GetAllCatalogsQuery : IQuery<ProductCatalogListViewModel>
    {
        
    }

    public class GetAllCatalogsQueryHandler : IQueryAsyncHandler<GetAllCatalogsQuery, ProductCatalogListViewModel>
    {
        private readonly IMongoCollection<ProductCatalogViewModel> _productCatalogViewCollection;

        public GetAllCatalogsQueryHandler(IMongoClient mongoClient)
        {
            IMongoDatabase mongoDb = mongoClient.GetDatabase(Constants.MongoDatabaseName);
            _productCatalogViewCollection = mongoDb.GetCollection<ProductCatalogViewModel>(nameof(ProductCatalogViewModel));
        }

        public async Task<ProductCatalogListViewModel> HandleAsync(GetAllCatalogsQuery query, CancellationToken cancellationToken = default(CancellationToken))
        {
            List<ProductCatalogViewModel> catalogs = await _productCatalogViewCollection.AsQueryable().ToListAsync(cancellationToken);
            return new ProductCatalogListViewModel()
            {
                Catalogs = catalogs
            };
        }
    }
}