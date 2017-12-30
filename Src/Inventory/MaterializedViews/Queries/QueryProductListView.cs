using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Xer.Cqrs.QueryStack;

namespace ViewModels.Queries
{
    public class QueryProductListView : IQuery<ProductListViewModel>
    {
        public bool IncludeNotForSaleProducts { get; }

        public QueryProductListView(bool includeNotForSale = true)
        {
            IncludeNotForSaleProducts = includeNotForSale;
        }
    }

    public class QueryProductListViewHandler : IQueryAsyncHandler<QueryProductListView, ProductListViewModel>
    {
        private readonly IMongoCollection<ProductViewModel> _productViewColection;

        public QueryProductListViewHandler(QueryMongoDatabase mongoDb)
        {
            _productViewColection = mongoDb.GetCollection<ProductViewModel>(nameof(ProductViewModel));
        }

        public async Task<ProductListViewModel> HandleAsync(QueryProductListView query, CancellationToken cancellationToken = default(CancellationToken))
        {
            if(!query.IncludeNotForSaleProducts)
            {
                var filter = Builders<ProductViewModel>.Filter.Eq(pv => pv.IsProductForSale, true);
                var forSaleProductViews = await _productViewColection.FindAsync(filter, null, cancellationToken);
                
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