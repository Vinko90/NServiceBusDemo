using NServiceBus;

namespace DA.Core.Events
{
    public class OrderBilled : IEvent
    {
        public string OrderId { get; set; }
    }
}
