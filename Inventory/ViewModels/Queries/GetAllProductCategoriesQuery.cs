using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Xer.Cqrs.QueryStack;

namespace ViewModels.Queries
{
    public class GetAllProductCategoriesQuery : IQuery<ProductCategoryListViewModel>
    {
        
    }

    public class GetAllProductCategoriesQueryHandler : IQueryAsyncHandler<GetAllProductCategoriesQuery, ProductCategoryListViewModel>
    {
        private readonly IMongoCollection<ProductCategoryViewModel> _categoryViewCollection;

        public GetAllProductCategoriesQueryHandler(QueryMongoDatabase mongoDb)
        {
            _categoryViewCollection = mongoDb.GetCollection<ProductCategoryViewModel>(nameof(ProductCategoryViewModel));
        }

        public async Task<ProductCategoryListViewModel> HandleAsync(GetAllProductCategoriesQuery query, CancellationToken cancellationToken = default(CancellationToken))
        {
            List<ProductCategoryViewModel> categories = await _categoryViewCollection.AsQueryable().ToListAsync(cancellationToken);
            return new ProductCategoryListViewModel()
            {
                Categories = categories
            };
        }
    }
}