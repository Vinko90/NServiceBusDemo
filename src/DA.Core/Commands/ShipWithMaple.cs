using NServiceBus;

namespace DA.Core.Commands;

public class ShipWithMaple : ICommand
{
    public string OrderId { get; set; }
}