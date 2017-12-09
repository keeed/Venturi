using System.Threading;
using System.Threading.Tasks;

namespace Domain.Repositories
{
    public interface IRepository<T, TId>
    {
         Task SaveAsync(T product, CancellationToken ct = default(CancellationToken));
         Task<T> GetByIdAsync(TId productId, CancellationToken ct = default(CancellationToken));
    }
}