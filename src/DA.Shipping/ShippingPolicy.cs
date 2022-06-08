using DA.Core.Commands;
using DA.Core.Events;
using NServiceBus;
using NServiceBus.Logging;

namespace DA.Shipping
{
    public class ShippingPolicy : 
        Saga<ShippingPolicyData>,
        IAmStartedByMessages<OrderPlaced>, // I can start the saga!
        IAmStartedByMessages<OrderBilled>  // I can start the saga too!
    {
        static ILog log = LogManager.GetLogger<ShippingPolicy>();

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<ShippingPolicyData> mapper)
        {
            mapper.MapSaga(sagaData => sagaData.OrderId)
                .ToMessage<OrderPlaced>(message => message.OrderId)
                .ToMessage<OrderBilled>(message => message.OrderId);
        }

        public Task Handle(OrderPlaced message, IMessageHandlerContext context)
        {
            log.Info($"[Order: {message.OrderId}] Received OrderPlaced");
            Data.IsOrderPlaced = true;
            return ProcessOrder(context);
        }

        public Task Handle(OrderBilled message, IMessageHandlerContext context)
        {
            log.Info($"[Order: {message.OrderId}] Received OrderBilled");
            Data.IsOrderBilled = true;
            return ProcessOrder(context);
        }

        private async Task ProcessOrder(IMessageHandlerContext context)
        {
            if (Data.IsOrderPlaced && Data.IsOrderBilled)
            {
                await context.SendLocal(new ShipOrder() { OrderId = Data.OrderId });
                MarkAsComplete();
            }
        }
    }
}
