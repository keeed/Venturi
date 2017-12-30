using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Exceptions;
using Domain.Repositories;
using Xer.Cqrs.EventStack;

namespace Domain.DomainEvents
    {
        public class UserSignInFailedEvent : IEvent
        {
            public string Username { get; }

            public UserSignInFailedEvent(string username)
            {
                Username = username;
            }
        }

        public class UserSignInFailedEventHandler : IEventAsyncHandler<UserSignInFailedEvent>
            {
                private readonly IUserRepository _userRepository;
                private readonly int _maximumFailedSignInAttempts;
                private readonly TimeSpan _lockoutDuration;

                public UserSignInFailedEventHandler(IUserRepository userRepository, int maximumAllowedFailedSignInAttempts, TimeSpan lockoutDuration)
                {
                    _userRepository = userRepository;
                    _lockoutDuration = lockoutDuration;
                    _maximumFailedSignInAttempts = maximumAllowedFailedSignInAttempts;
                }

                public async Task HandleAsync(UserSignInFailedEvent @event, CancellationToken cancellationToken = default(CancellationToken))
        {
            User user = await _userRepository.GetUserByUsernameAsync(@event.Username, cancellationToken);
            if(user != null)
            {
                if(user.HasExceededMaxFailedSignInAttempts(5))
                {
                    user.Lockout(_lockoutDuration);
                    await _userRepository.SaveAsync(user, cancellationToken);
                }
            }
        }
    }
}