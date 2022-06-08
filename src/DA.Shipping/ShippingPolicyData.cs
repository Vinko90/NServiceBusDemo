using NServiceBus;

namespace DA.Shipping
{
    public class ShippingPolicyData : ContainSagaData
    {
        public string OrderId { get; set; }

        public bool IsOrderPlaced { get; set; }

        public bool IsOrderBilled { get; set; }
    }
}
