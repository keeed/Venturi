using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Domain.IntegrationEvents.Orders;
using Newtonsoft.Json;

namespace Infrastructure.Integration.Orders
{
    public class OrderStockConfirmationQueueSender : IOrderStockConfirmationSender
    {
        public Task SendAsync(OrderSenderSystem system, OrderStockConfirmation confirmation)
        {
            string json = JsonConvert.SerializeObject(confirmation);

            var savePath = Path.Combine(system.ReplyAddress, Guid.NewGuid().ToString());

            Directory.CreateDirectory(system.ReplyAddress);
            File.AppendAllText(savePath, json);

            return Task.CompletedTask;
        }
    }
}