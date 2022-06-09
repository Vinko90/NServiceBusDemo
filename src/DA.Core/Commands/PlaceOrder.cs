using NServiceBus;

namespace DA.Core.Command;

public class PlaceOrder : ICommand
{
    public string OrderId { get; set; }

}