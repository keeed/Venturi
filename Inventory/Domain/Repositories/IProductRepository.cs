using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Repositories
{
    public interface IProductRepository : IRepository<Product, ProductId>
    {
        Task DeleteByIdAsync(ProductId productId, CancellationToken ct = default(CancellationToken));
    }
}