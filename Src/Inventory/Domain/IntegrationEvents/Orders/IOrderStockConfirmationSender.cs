using System;
using System.Threading.Tasks;

namespace Domain.IntegrationEvents.Orders
{
    public interface IOrderStockConfirmationSender
    {
        Task SendAsync(OrderSenderSystem system, OrderStockConfirmation confirmation);
    }

    public class OrderStockConfirmation
    {
        public Guid ProductId { get; }
        public bool IsStockConfirmed { get; }

        public OrderStockConfirmation(Guid productId, bool isStockConfirmed)
        {
            ProductId = productId;
            IsStockConfirmed = isStockConfirmed;
        }
    }
}