using DA.Core.Commands;
using DA.Core.Messages;
using NServiceBus;
using NServiceBus.Logging;

namespace DA.Shipping;

public class ShipWithMapleHandler : IHandleMessages<ShipWithMaple>
{
    private static readonly ILog Log = LogManager.GetLogger<ShipWithMapleHandler>();

    private const int MaximumTimeMapleMightRespond = 60;
    private static readonly Random Random = new();

    public async Task Handle(ShipWithMaple message, IMessageHandlerContext context)
    {
        var waitingTime = Random.Next(MaximumTimeMapleMightRespond);

        Log.Info($"[Order: {message.OrderId}] ShipWithMapleHandler: Delaying Order {waitingTime} seconds.");

        await Task.Delay(waitingTime * 1000);

        await context.Reply(new ShipmentAcceptedByMaple());
    }
}