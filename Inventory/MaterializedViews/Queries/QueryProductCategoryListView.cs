using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Xer.Cqrs.QueryStack;

namespace ViewModels.Queries
{
    public class QueryProductCategoryListView : IQuery<ProductCategoryListViewModel>
    {
        
    }

    public class QueryProductCategoryListViewHandler : IQueryAsyncHandler<QueryProductCategoryListView, ProductCategoryListViewModel>
    {
        private readonly IMongoCollection<ProductCategoryViewModel> _categoryViewCollection;

        public QueryProductCategoryListViewHandler(QueryMongoDatabase mongoDb)
        {
            _categoryViewCollection = mongoDb.GetCollection<ProductCategoryViewModel>(nameof(ProductCategoryViewModel));
        }

        public async Task<ProductCategoryListViewModel> HandleAsync(QueryProductCategoryListView query, CancellationToken cancellationToken = default(CancellationToken))
        {
            List<ProductCategoryViewModel> categories = await _categoryViewCollection.AsQueryable().ToListAsync(cancellationToken);
            return new ProductCategoryListViewModel()
            {
                Categories = categories
            };
        }
    }
}