namespace DA.Core.Constants
{
    public static class ServiceNames
    {
        public const string ClientServiceName   = "DA.ClientUI";
        public const string BillingServiceName  = "DA.Billing";
        public const string SalesServiceName    = "DA.Sales";
        public const string ShippingServiceName = "DA.Shipping";
    }

    public static class ConnectionString
    {
        public const string RabbitMQConnectionString = "host=localhost";

        public const string SagaDatabaseName = "DABusDemo";

        public const string MongoDBConnectionString = "mongodb://localhost:27017";

        public const string MSSQLConnectionString = $"Server=localhost,15789;Database={SagaDatabaseName};User Id=sa;Password=Testing1122;Trust Server Certificate = true";
    }
}
