using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Repositories
{
    public class UserInMemoryRepository : IUserRepository
    {
        private readonly List<User> _users = new List<User>();

        public Task<List<User>> GetLockedOutUsersAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(_users.Where(u => u.IsLockedOut).ToList());
        }

        public Task<User> GetUserByUsernameAsync(string username, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(_users.FirstOrDefault(u => u.Username == username));
        }

        public Task SaveAsync(User user, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (user == null)
            {
                throw new System.ArgumentNullException(nameof(user));
            }

            _users.RemoveAll(u => u.Username == user.Username);
            _users.Add(user);

            return Task.CompletedTask;
        }
    }
}