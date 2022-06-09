using NServiceBus;

namespace DA.Sales;

public class BuyersRemorseState : ContainSagaData
{
    public string OrderId { get; set; }
}