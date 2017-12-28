using System.Threading;
using System.Threading.Tasks;
using Domain.DomainEvents;
using Microsoft.Extensions.Hosting;

namespace Api.HostedServices
{
    public class UserLockoutReleaseHostedService : IHostedService
    {
        private readonly UserLockoutTimeElapsedEventHandler _hostedHandler;

        public UserLockoutReleaseHostedService(UserLockoutTimeElapsedEventHandler handler)
        {
            _hostedHandler = handler;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        { 
            return _hostedHandler.Start(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return _hostedHandler.Stop(cancellationToken);
        }
    }
}