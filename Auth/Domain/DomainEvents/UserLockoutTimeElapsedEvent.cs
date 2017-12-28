using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Domain.Repositories;
using Xer.Cqrs.EventStack;
using Xer.Cqrs.EventStack.Hosted;

namespace Domain.DomainEvents
{
    public class UserLockoutTimeElapsedEvent : IEvent
    {
        public string Username { get; }

        public UserLockoutTimeElapsedEvent(string username)
        {
            Username = username;
        }
    }

    public class UserLockoutTimeElapsedEventSource : Xer.Cqrs.EventStack.Hosted.IEventSource
    {
        private readonly IUserRepository _userRepository;
        private readonly int _releaseInterval;
        private System.Timers.Timer _timer;

        public event EventHandlerDelegate EventReceived;

        public UserLockoutTimeElapsedEventSource(IUserRepository userRepository, int releaseInterval)
        {
            _releaseInterval = releaseInterval;
            _userRepository = userRepository;
        }

        public Task Receive(IEvent @event)
        {
            if(EventReceived != null)
            {
                EventReceived(@event);
            }

            return Task.CompletedTask;
        }

        public Task StartReceiving(CancellationToken cancellationToken = default(CancellationToken))
        {
            _timer = new System.Timers.Timer();
            _timer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimerElapsed);
            _timer.Interval = _releaseInterval;
            
            _timer.Start();
            return Task.CompletedTask;
        }

        public Task StopReceiving(CancellationToken cancellationToken = default(CancellationToken))
        {
            _timer.Stop();
            return Task.CompletedTask;
        }

        private async void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                List<User> lockedOutUsers = await _userRepository.GetLockedOutUsersAsync();

                if(EventReceived != null)
                {
                    foreach(User user in lockedOutUsers)
                    {
                        // Not awaited.
                        Task t = EventReceived(new UserLockoutTimeElapsedEvent(user.Username));
                    }
                }
            }
            catch(Exception)
            {
                // TODO: Log.
            }
        }
    }

    public class UserLockoutTimeElapsedEventHandler : HostedEventHandler<UserLockoutTimeElapsedEvent>
    {
        private readonly IUserRepository _userRepository;

        protected override Xer.Cqrs.EventStack.Hosted.IEventSource EventSource { get; }

        public UserLockoutTimeElapsedEventHandler(IUserRepository userRepository, int releaseInterval)
        {
            _userRepository = userRepository;
            EventSource = new UserLockoutTimeElapsedEventSource(userRepository, releaseInterval);
        }
        
        protected override async Task ProcessEventAsync(UserLockoutTimeElapsedEvent receivedEvent, CancellationToken cancellationToken)
        {
            User lockedOutUser = await _userRepository.GetUserByUsernameAsync(receivedEvent.Username, cancellationToken).ConfigureAwait(false);
            if(lockedOutUser != null)
            {
                lockedOutUser.ReleaseLock();
            }
        }
    }
}