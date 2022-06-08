using NServiceBus;

namespace DA.Core.Commands
{
    public class CancelOrder : ICommand
    {
        public string OrderId { get; set; }
    }
}
