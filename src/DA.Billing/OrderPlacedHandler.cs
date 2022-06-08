using DA.Core.Events;
using NServiceBus;
using NServiceBus.Logging;

namespace DA.Billing
{
    public class OrderPlacedHandler : IHandleMessages<OrderPlaced>
    {
        static ILog log = LogManager.GetLogger<OrderPlacedHandler>();

        public Task Handle(OrderPlaced message, IMessageHandlerContext context)
        {
            log.Info($"[Order: {message.OrderId}] Received OrderPlaced, credi card charged...Sending OrderBilled");

            var orderBilled = new OrderBilled
            {
                OrderId = message.OrderId
            };
            return context.Publish(orderBilled);
        }
    }
}
