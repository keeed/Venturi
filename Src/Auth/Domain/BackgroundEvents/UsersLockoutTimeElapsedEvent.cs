using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Domain.Repositories;
using Xer.Cqrs.EventStack;
using Xer.Cqrs.EventStack.Hosted;
using Xer.Cqrs.EventStack.Hosted.EventSources;

namespace Domain.DomainEvents
{
    public class UsersLockoutTimeElapsedEvent : IEvent
    {
        public IReadOnlyCollection<string> LockedOutUsernames { get; }

        public UsersLockoutTimeElapsedEvent(IEnumerable<string> usernames)
        {
            LockedOutUsernames = usernames.ToList().AsReadOnly();
        }
    }

    #region Event Source

    public class UsersLockoutTimeElapsedEventSource : PollingEventSource
    {
        private readonly IUserRepository _userRepository;

        protected override TimeSpan Interval { get; }

        public UsersLockoutTimeElapsedEventSource(IUserRepository userRepository, TimeSpan releaseInterval)
        {
            _userRepository = userRepository;
            Interval = releaseInterval;
        }

        protected override async Task<IEvent> GetNextEventAsync(CancellationToken cancellationToken)
        {
            List<User> lockedOutUsers = await _userRepository.GetLockedOutUsersAsync();

            if(lockedOutUsers.Count > 0)
            {
                // Put all usernames of locked-out users to the event.
                return new UsersLockoutTimeElapsedEvent(lockedOutUsers.Select(u => u.GetCurrentState().Username));
            }

            return null;
        }
    }

    #endregion Event Source

    #region Hosted Event Handler

    public class UsersLockoutTimeElapsedHostedEventHandler : HostedEventHandler<UsersLockoutTimeElapsedEvent>
    {
        private readonly IUserRepository _userRepository;

        protected override IEventSource EventSource { get; }

        public UsersLockoutTimeElapsedHostedEventHandler(IUserRepository userRepository, TimeSpan releaseInterval)
        {
            _userRepository = userRepository;
            EventSource = new UsersLockoutTimeElapsedEventSource(userRepository, releaseInterval);
        }
        
        protected override async Task ProcessEventAsync(UsersLockoutTimeElapsedEvent receivedEvent, CancellationToken cancellationToken)
        {
            foreach(string username in receivedEvent.LockedOutUsernames)
            {
                User lockedOutUser = await _userRepository.GetUserByUsernameAsync(username, cancellationToken).ConfigureAwait(false);
                if (lockedOutUser != null)
                {
                    UserState state = lockedOutUser.GetCurrentState();

                    if(state.LockoutExpiry.HasValue && state.LockoutExpiry.Value > DateTime.Now)
                    {
                        // Expiry has elapsed.
                        lockedOutUser.ReleaseLock();
                    }
                }
            }
        }
    }

    #endregion Hosted Event Handler
}