using System.Threading;
using System.Threading.Tasks;

namespace Domain.Repositories
{
    public interface IInventoryRepository : IRepository<Inventory, InventoryId>
    {
    }
}