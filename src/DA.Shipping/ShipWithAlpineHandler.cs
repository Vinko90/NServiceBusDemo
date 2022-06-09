using DA.Core.Commands;
using DA.Core.Messages;
using NServiceBus;
using NServiceBus.Logging;

namespace DA.Shipping;

class ShipWithAlpineHandler : IHandleMessages<ShipWithAlpine>
{
    private static readonly ILog Log = LogManager.GetLogger<ShipWithAlpineHandler>();

    private const int MaximumTimeAlpineMightRespond = 30;
    private static readonly Random Random = new();

    public async Task Handle(ShipWithAlpine message, IMessageHandlerContext context)
    {
        var waitingTime = Random.Next(MaximumTimeAlpineMightRespond);

        Log.Info($"[Order: {message.OrderId}] ShipWithAlpineHandler: Delaying Order {waitingTime} seconds.");

        await Task.Delay(waitingTime * 1000);

        await context.Reply(new ShipmentAcceptedByAlpine());
    }
}