using NServiceBus;

namespace DA.Core.Events;

public class ShipmentFailed : IEvent
{
    public string OrderId { get; set; }
}