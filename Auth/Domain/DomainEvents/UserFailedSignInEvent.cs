using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Exceptions;
using Domain.Repositories;
using Xer.Cqrs.EventStack;

namespace Domain.DomainEvents
{
    public class UserFailedSignInEvent : IEvent
    {
        public string Username { get; }

        public UserFailedSignInEvent(string username)
        {
            Username = username;
        }
    }

    public class UserFailedSignInEventHandler : IEventAsyncHandler<UserFailedSignInEvent>
    {
        private readonly IUserRepository _userRepository;

        public UserFailedSignInEventHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task HandleAsync(UserFailedSignInEvent @event, CancellationToken cancellationToken = default(CancellationToken))
        {
            User user = await _userRepository.GetUserByUsernameAsync(@event.Username, cancellationToken);
            if(user == null)
            {
                throw new UserNotFoundException("User not found.");
            }

            if(user.HasReachedMaximumSignInAttempts(5))
            {
                user.Lockout(TimeSpan.FromMinutes(5));
            }
        }
    }
}