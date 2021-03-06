using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.DomainEvents;
using Domain.Repositories;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Api.HostedServices
{
    public class LockoutReleaseHostedService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private UsersLockoutTimeElapsedHostedEventHandler _hostedHandler;

        public LockoutReleaseHostedService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _hostedHandler = _serviceProvider.GetRequiredService<UsersLockoutTimeElapsedHostedEventHandler>();
            
            return _hostedHandler.Start(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return _hostedHandler.Stop(cancellationToken);
        }
    }
}