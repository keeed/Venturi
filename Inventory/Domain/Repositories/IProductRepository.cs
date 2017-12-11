using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Repositories
{
    public interface IProductRepository : IRepository<Product, ProductId>
    {
        //List<Product> GetProductsInCatalogAsync(string catalogName, CancellationToken ct = default(CancellationToken));
    }
}