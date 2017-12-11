using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Xer.Cqrs.QueryStack;

namespace ViewModels.Queries
{
    public class GetAllProductsQuery : IQuery<ProductListView>
    {
        public bool IncludeNotForSaleProducts { get; }

        public GetAllProductsQuery(bool includeNotForSale = true)
        {
            IncludeNotForSaleProducts = includeNotForSale;
        }
    }

    public class GetAllProductsQueryHandler : IQueryAsyncHandler<GetAllProductsQuery, ProductListView>
    {
        private readonly IMongoCollection<ProductView> _productViewColection;

        public GetAllProductsQueryHandler(IMongoClient mongoClient)
        {
            IMongoDatabase mongoDb = mongoClient.GetDatabase(Constants.MongoDatabaseName);
            _productViewColection = mongoDb.GetCollection<ProductView>(nameof(ProductView));
        }

        public async Task<ProductListView> HandleAsync(GetAllProductsQuery query, CancellationToken cancellationToken = default(CancellationToken))
        {
            if(!query.IncludeNotForSaleProducts)
            {
                var filter = Builders<ProductView>.Filter.Eq(pv => pv.IsProductForSale, true);
                IAsyncCursor<ProductView> forSaleProductViews = await _productViewColection.FindAsync(filter, null, cancellationToken);
                
                return new ProductListView()
                {
                    Products = await forSaleProductViews.ToListAsync(cancellationToken)
                };
            }
            else
            {
                return new ProductListView()
                {
                    Products = await _productViewColection.AsQueryable().ToListAsync(cancellationToken)
                };
            }
        }
    }
}