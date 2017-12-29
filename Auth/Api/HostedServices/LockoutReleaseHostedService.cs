using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.DomainEvents;
using Domain.Repositories;
using Microsoft.Extensions.Hosting;

namespace Api.HostedServices
{
    public class LockoutReleaseHostedService : UsersLockoutTimeElapsedHostedEventHandler, IHostedService
    {
        public LockoutReleaseHostedService(IUserRepository userRepository, TimeSpan releaseInterval) 
            : base(userRepository, releaseInterval)
        {
        }

        public Task StartAsync(CancellationToken cancellationToken)
        { 
            return base.Start(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return base.Stop(cancellationToken);
        }
    }
}