using System.Threading;
using System.Threading.Tasks;

namespace Domain.Repositories
{
    public interface IRepository<T, TId>
    {
         Task SaveAsync(T entity, CancellationToken ct = default(CancellationToken));
         Task<T> GetByIdAsync(TId entityId, CancellationToken ct = default(CancellationToken));
    }
}