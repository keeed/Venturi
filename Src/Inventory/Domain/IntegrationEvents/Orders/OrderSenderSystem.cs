namespace Domain.IntegrationEvents.Orders
{
    public class OrderSenderSystem
    {
        public string Name { get; }
        public string ReplyAddress { get; }

        public OrderSenderSystem(string name, string replyAddress)
        {
            Name = name;
            ReplyAddress = replyAddress;
        }
    }
}