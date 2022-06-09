using NServiceBus;

namespace DA.Core.Events;

public class OrderPlaced : IEvent
{
    public string OrderId { get; set; }
}