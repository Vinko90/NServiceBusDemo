using DA.Core.Command;
using DA.Core.Commands;
using DA.Core.Events;
using NServiceBus;
using NServiceBus.Logging;

namespace DA.Sales;

internal class BuyersRemorsePolicy : 
    Saga<BuyersRemorseState>,
    IAmStartedByMessages<PlaceOrder>,
    IHandleMessages<CancelOrder>,
    IHandleTimeouts<BuyersRemorseIsOver>
{
    static ILog log = LogManager.GetLogger<BuyersRemorsePolicy>();

    protected override void ConfigureHowToFindSaga(SagaPropertyMapper<BuyersRemorseState> mapper)
    {
        mapper.MapSaga(saga => saga.OrderId)
            .ToMessage<PlaceOrder>(message => message.OrderId)
            .ToMessage<CancelOrder>(message => message.OrderId);
    }

    public async Task Handle(PlaceOrder message, IMessageHandlerContext context)
    {
        log.Info($"[Order: {Data.OrderId}] Received PlaceOrder, Starting 10s cool down period...");
            
        //Data.OrderId = message.OrderId;
            
        await RequestTimeout(context, TimeSpan.FromSeconds(10), new BuyersRemorseIsOver());
    }

    public async Task Timeout(BuyersRemorseIsOver timeout, IMessageHandlerContext context)
    {
        log.Info($"[Order: {Data.OrderId}] Cooling down period has elapsed. Sending OrderPlaced");

        var orderPlaced = new OrderPlaced
        {
            OrderId = Data.OrderId
        };

        await context.Publish(orderPlaced);

        MarkAsComplete();
    }

    public Task Handle(CancelOrder message, IMessageHandlerContext context)
    {
        log.Info($"[Order: {message.OrderId}] Order was cancelled.");

        //Possibly publish an OrderCancelled event?

        MarkAsComplete();

        return Task.CompletedTask;
    }
}

//Message DTO needed to handle buyers remorse timeout.
//Used only here internally on the saga.
internal class BuyersRemorseIsOver
{
}