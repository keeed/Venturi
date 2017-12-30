using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xer.Cqrs.EventStack;

namespace Domain.Repositories
{
    public class PublishingUserRepository : IUserRepository
    {
        private readonly IUserRepository _inner;
        private readonly IEventPublisher _eventPublisher;

        public PublishingUserRepository(IUserRepository inner, IEventPublisher eventPublisher)
        {
            _inner = inner;
            _eventPublisher = eventPublisher;
        }

        public Task<User> GetUserByUsernameAsync(string username, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _inner.GetUserByUsernameAsync(username, cancellationToken);
        }

        public Task<List<User>> GetLockedOutUsersAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return _inner.GetLockedOutUsersAsync(cancellationToken);
        }

        public async Task SaveAsync(User user, CancellationToken cancellationToken = default(CancellationToken))
        {
            IEnumerable<IEvent> events = user.GetEvents();

            await _inner.SaveAsync(user, cancellationToken).ConfigureAwait(false);

            if(events.Any())
            {
                await _eventPublisher.PublishAsync(events).ConfigureAwait(false);
            }
            
            user.ClearEvents();
        }
    }
}