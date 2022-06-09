using DA.Core.Commands;
using NServiceBus;
using NServiceBus.Logging;

namespace DA.Shipping;

internal class ShipOrderHandler : IHandleMessages<ShipOrder>
{
    static ILog log = LogManager.GetLogger<ShipOrderHandler>();

    public Task Handle(ShipOrder message, IMessageHandlerContext context)
    {
        log.Info($"[Order: {message.OrderId}] Order successfully shipped.");
        return Task.CompletedTask;
    }
}