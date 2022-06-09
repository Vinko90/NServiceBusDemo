using System.Runtime.InteropServices;
using DA.Core.Commands;
using DA.Core.Constants;
using Microsoft.Data.SqlClient;
using NServiceBus;

namespace DA.Shipping;

internal static class Program
{
    private static async Task Main()
    {
        Console.Title = ServiceNames.ShippingServiceName;

        //Set service endpoint name
        var endpointConfiguration = new EndpointConfiguration(ServiceNames.ShippingServiceName);

        //Automatically create queues if not exist
        endpointConfiguration.EnableInstallers();

        //Use RabbitMQ as transport
        var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
        transport.UseConventionalRoutingTopology();
        transport.ConnectionString(ConnectionString.RabbitMQConnectionString);

        //Routing Configuration
        var routing = transport.Routing();
        routing.RouteToEndpoint(typeof(ShipOrder), ServiceNames.ShippingServiceName);
        routing.RouteToEndpoint(typeof(ShipWithMaple), ServiceNames.ShippingServiceName);
        routing.RouteToEndpoint(typeof(ShipWithAlpine), ServiceNames.ShippingServiceName);

        //Add Saga persistence to MongoDB
        //var persistence = endpointConfiguration.UsePersistence<MongoPersistence>();
        //persistence.MongoClient(new MongoClient(ConnectionString.MongoDBConnectionString));
        //persistence.DatabaseName(ConnectionString.MongoDatabaseName);
        //persistence.UseTransactions(false);

        //Add Saga persistence to MSSQL
        var persistence = endpointConfiguration.UsePersistence<SqlPersistence>();
        var subscriptions = persistence.SubscriptionSettings();
        subscriptions.CacheFor(TimeSpan.FromMinutes(1));
        persistence.SqlDialect<SqlDialect.MsSqlServer>();
        persistence.ConnectionBuilder(connectionBuilder: () => new SqlConnection(ConnectionString.MSSQLConnectionString));

        //In OSX we don't have ServiceControl
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            //Configure monitoring system
            var json = await File.ReadAllTextAsync("ServicePulseConfig.json");
            var servicePlatformConnection = ServicePlatformConnectionConfiguration.Parse(json);
            endpointConfiguration.ConnectToServicePlatform(servicePlatformConnection);
        }

        //Start bus
        var endpointInstance = await Endpoint.Start(endpointConfiguration).ConfigureAwait(false);

        Console.WriteLine("Press Enter to exit.");
        Console.ReadLine();

        //Stop bus
        await endpointInstance.Stop().ConfigureAwait(false);
    }
}