using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Repositories;
using Xer.Cqrs.EventStack;
using Xer.Cqrs.EventStack.Hosted;
using Xer.Cqrs.EventStack.Hosted.EventSources;

namespace Domain.IntegrationEvents.Orders
{
    public class OrderPlacedEvent : IEvent
    {
        public Guid OrderId { get; }
        public Guid ProductId { get; }
        public int OrderQuantity { get; }
        public OrderSenderSystem OrderSenderSystem { get; }

        public OrderPlacedEvent(Guid orderId, Guid productId, int orderQuantity, OrderSenderSystem orderSenderSystem)
        {
            OrderId = orderId;
            ProductId = productId;
            OrderQuantity = orderQuantity;
            OrderSenderSystem = orderSenderSystem;
        }
    }

    public class OrderPlacedHostedEventHandler : HostedEventHandler<OrderPlacedEvent>
    {
        private readonly IProductRepository _productRepository;
        private readonly IOrderStockConfirmationSender _orderConfirmationSender;

        protected override IEventSource EventSource { get; }

        public OrderPlacedHostedEventHandler(
            OrderPlacedEventSource eventSource, 
            IProductRepository productRepository, 
            IOrderStockConfirmationSender orderConfirmationSender
        )
        {
            _productRepository = productRepository;
            _orderConfirmationSender = orderConfirmationSender;
            EventSource = eventSource;
        }

        protected override async Task ProcessEventAsync(OrderPlacedEvent receivedEvent, CancellationToken cancellationToken)
        {
            Product productOrdered = await _productRepository.GetProductByIdAsync(new ProductId(receivedEvent.ProductId), cancellationToken).ConfigureAwait(false);
            if (productOrdered != null)
            {
                if (productOrdered.HasQuantityInStock(receivedEvent.OrderQuantity))
                {
                    productOrdered.DecreaseProductStock(receivedEvent.OrderQuantity);

                    // In stock.
                    await _orderConfirmationSender.SendAsync(receivedEvent.OrderSenderSystem, new OrderStockConfirmation(receivedEvent.ProductId, true));
                }
                else
                {
                    // Not in stock.
                    await _orderConfirmationSender.SendAsync(receivedEvent.OrderSenderSystem, new OrderStockConfirmation(receivedEvent.ProductId, false));
                }
            }
        }
    }

    public abstract class OrderPlacedEventSource : PollingEventSource
    {
        public OrderPlacedEventSource(TimeSpan interval)
        {
            Interval = interval;
        }

        protected override TimeSpan Interval { get; }

        protected sealed override async Task<IEvent> GetNextEventAsync(CancellationToken cancellationToken)
        {
            OrderPlacedEvent orderPlacedEvent = await GetNextOrderPlacedEventAsync(cancellationToken);
            return orderPlacedEvent;
        }

        protected abstract Task<OrderPlacedEvent> GetNextOrderPlacedEventAsync(CancellationToken cancellationToken);
    }
}