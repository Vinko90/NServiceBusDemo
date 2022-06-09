using DA.Core.Commands;
using DA.Core.Events;
using DA.Core.Messages;
using NServiceBus;
using NServiceBus.Logging;

namespace DA.Shipping;

class ShipOrderWorkflow :
    Saga<ShipOrderWorkflow.ShipOrderData>,
    IAmStartedByMessages<ShipOrder>,
    IHandleMessages<ShipmentAcceptedByMaple>,
    IHandleMessages<ShipmentAcceptedByAlpine>,
    IHandleTimeouts<ShipOrderWorkflow.ShippingEscalation>

{
    static ILog log = LogManager.GetLogger<ShipOrderWorkflow>();

    protected override void ConfigureHowToFindSaga(SagaPropertyMapper<ShipOrderData> mapper)
    {
        mapper.MapSaga(saga => saga.OrderId)
            .ToMessage<ShipOrder>(message => message.OrderId);
    }
    
    public async Task Handle(ShipOrder message, IMessageHandlerContext context)
    {
        log.Info($"[Order: {Data.OrderId}] ShipOrderWorkflow -> Trying Maple first.");
        
        // Execute order to ship with Maple
        await context.Send(new ShipWithMaple { OrderId = Data.OrderId });

        // Add timeout to escalate if Maple did not ship in time.
        await RequestTimeout(context, TimeSpan.FromSeconds(20), new ShippingEscalation());
    }
    
    public Task Handle(ShipmentAcceptedByMaple message, IMessageHandlerContext context)
    {
        if (Data.ShipmentOrderSentToAlpine) return Task.CompletedTask;
        
        log.Info($"[Order: {Data.OrderId}] Successfully shipped with Maple");

        Data.ShipmentAcceptedByMaple = true;

        MarkAsComplete();

        return Task.CompletedTask;
    }
    
    public Task Handle(ShipmentAcceptedByAlpine message, IMessageHandlerContext context)
    {
        log.Info($"[Order: {Data.OrderId}] Successfully shipped with Alpine");

        Data.ShipmentAcceptedByAlpine = true;

        MarkAsComplete();

        return Task.CompletedTask;
    }
    
    public async Task Timeout(ShippingEscalation timeout, IMessageHandlerContext context)
    {
        if (!Data.ShipmentAcceptedByMaple)
        {
            if (!Data.ShipmentOrderSentToAlpine)
            {
                log.Info($"[Order: {Data.OrderId}] No answer from Maple, let's try Alpine...");
                Data.ShipmentOrderSentToAlpine = true;
                await context.Send(new ShipWithAlpine() { OrderId = Data.OrderId });
                await RequestTimeout(context, TimeSpan.FromSeconds(20), new ShippingEscalation());
            }
            else if (!Data.ShipmentAcceptedByAlpine) // No response from Maple nor Alpine
            {
                log.Warn($"[Order: {Data.OrderId}] No answer from Maple/Alpine. We need to escalate!");

                // escalate to Warehouse Manager!
                await context.Publish<ShipmentFailed>();

                MarkAsComplete();
            }
        }
    }
    
    /// <summary>
    /// Saga Data - Keep internal
    /// </summary>
    internal class ShipOrderData : ContainSagaData
    {
        public string OrderId { get; set; }
        
        public bool ShipmentAcceptedByMaple { get; set; }
        
        public bool ShipmentOrderSentToAlpine { get; set; }
        
        public bool ShipmentAcceptedByAlpine { get; set; }
    }
    
    /// <summary>
    /// Internal Class used for escalation timeout
    /// </summary>
    internal class ShippingEscalation
    {
    }
}