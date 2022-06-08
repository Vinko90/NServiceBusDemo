using NServiceBus;

namespace DA.Core.Commands
{
    public class ShipOrder : ICommand
    {
        public string OrderId { get; set; }
    }
}
