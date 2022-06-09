using DA.Core.Commands;
using DA.Core.Events;
using NServiceBus;
using NServiceBus.Logging;

namespace DA.Shipping;

class ShippingPolicy : 
    Saga<ShippingPolicy.ShippingPolicyData>,
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
    
    
    /// <summary>
    /// Shipping Policy Data -> Keep Internal
    /// </summary>
    internal class ShippingPolicyData : ContainSagaData
    {
        public string OrderId { get; set; }

        public bool IsOrderPlaced { get; set; }

        public bool IsOrderBilled { get; set; }
    }
}