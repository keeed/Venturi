using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Repositories
{
    public interface IProductRepository
    {
        Task<Product> GetProductByIdAsync(ProductId productId, CancellationToken ct = default(CancellationToken));
        Task SaveAsync(Product product, CancellationToken ct = default(CancellationToken));
    }
}