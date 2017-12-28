using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Repositories
{
    public interface IUserRepository
    {
        Task<User> GetUserByUsernameAsync(string username, CancellationToken cancellationToken = default(CancellationToken));
        Task<List<User>> GetLockedOutUsersAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task SaveAsync(User user, CancellationToken cancellationToken = default(CancellationToken));
    }
}