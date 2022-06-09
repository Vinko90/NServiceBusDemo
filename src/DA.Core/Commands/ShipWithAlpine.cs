using NServiceBus;

namespace DA.Core.Commands;

public class ShipWithAlpine : ICommand
{
    public string OrderId { get; set; }
}