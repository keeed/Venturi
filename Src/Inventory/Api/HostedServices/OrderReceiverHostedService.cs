using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.IntegrationEvents.Orders;
using Domain.Repositories;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Api.HostedServices
{
    public class OrderReceiverHostedService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private OrderPlacedHostedEventHandler _hostedHandler;

        public OrderReceiverHostedService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _hostedHandler = _serviceProvider.GetRequiredService<OrderPlacedHostedEventHandler>();
            
            return _hostedHandler.Start(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return _hostedHandler.Stop(cancellationToken);
        }
    }
}