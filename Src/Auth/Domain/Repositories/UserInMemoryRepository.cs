using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Repositories
{
    public class UserInMemoryRepository : IUserRepository
    {
        private readonly List<UserState> _userStates = new List<UserState>();

        public Task<List<User>> GetLockedOutUsersAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            IEnumerable<UserState> lockedoutUserStates = _userStates.Where(us => us.IsLockedOut);
            IEnumerable<User> users = lockedoutUserStates.Select(us => new User(us));

            return Task.FromResult(users.ToList());
        }

        public Task<User> GetUserByUsernameAsync(string username, CancellationToken cancellationToken = default(CancellationToken))
        {
            UserState state = _userStates.FirstOrDefault(us => us.Username == username);

            User user = null;
            
            if(state != null)
            {
                user = new User(state);
            }

            return Task.FromResult(user);
        }

        public Task SaveAsync(User user, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (user == null)
            {
                throw new System.ArgumentNullException(nameof(user));
            }

            UserState currentState = user.GetCurrentState();

            _userStates.RemoveAll(us => us.Username == currentState.Username);
            _userStates.Add(currentState);

            return Task.CompletedTask;
        }
    }
}