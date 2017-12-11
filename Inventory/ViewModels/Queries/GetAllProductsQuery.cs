using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Xer.Cqrs.QueryStack;

namespace ViewModels.Queries
{
    public class GetAllProductsQuery : IQuery<ProductListViewModel>
    {
        public bool IncludeNotForSaleProducts { get; }

        public GetAllProductsQuery(bool includeNotForSale = true)
        {
            IncludeNotForSaleProducts = includeNotForSale;
        }
    }

    public class GetAllProductsQueryHandler : IQueryAsyncHandler<GetAllProductsQuery, ProductListViewModel>
    {
        private readonly IMongoCollection<ProductViewModel> _productViewColection;

        public GetAllProductsQueryHandler(IMongoClient mongoClient)
        {
            IMongoDatabase mongoDb = mongoClient.GetDatabase(Constants.MongoDatabaseName);
            _productViewColection = mongoDb.GetCollection<ProductViewModel>(nameof(ProductViewModel));
        }

        public async Task<ProductListViewModel> HandleAsync(GetAllProductsQuery query, CancellationToken cancellationToken = default(CancellationToken))
        {
            if(!query.IncludeNotForSaleProducts)
            {
                var filter = Builders<ProductViewModel>.Filter.Eq(pv => pv.IsProductForSale, true);
                IAsyncCursor<ProductViewModel> forSaleProductViews = await _productViewColection.FindAsync(filter, null, cancellationToken);
                
                return new ProductListViewModel()
                {
                    Products = await forSaleProductViews.ToListAsync(cancellationToken)
                };
            }
            else
            {
                return new ProductListViewModel()
                {
                    Products = await _productViewColection.AsQueryable().ToListAsync(cancellationToken)
                };
            }
        }
    }
}