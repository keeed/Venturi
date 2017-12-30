using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.IntegrationEvents.Orders;
using Newtonsoft.Json;

namespace Infrastructure.Integration.Orders
{
    public class OrderPlacedEventQueueSource : OrderPlacedEventSource
    {
        private const string OrderQueueDirectory = @"C:\Users\jeyjeyemem\Desktop\OrdersQueue";

        public OrderPlacedEventQueueSource(TimeSpan interval) 
            : base(interval)
        {
            if(!Directory.Exists(OrderQueueDirectory))
            {
                Directory.CreateDirectory(OrderQueueDirectory);
            }
        }

        protected override Task<OrderPlacedEvent> GetNextOrderPlacedEventAsync(CancellationToken cancellationToken)
        {
            var filePath = Directory.GetFiles(OrderQueueDirectory).FirstOrDefault();

            if (string.IsNullOrEmpty(filePath))
            {
                return Task.FromResult<OrderPlacedEvent>(null);
            }

            OrderPlacedEvent @event = JsonConvert.DeserializeObject<OrderPlacedEvent>(File.ReadAllText(filePath));

            File.Delete(filePath);

            return Task.FromResult(@event);
        }
    }
}