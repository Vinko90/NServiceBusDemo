using System.Runtime.InteropServices;
using DA.Core.Constants;
using NServiceBus;

namespace DA.Billing;

internal static class Program
{
    private static async Task Main()
    {
        Console.Title = ServiceNames.BillingServiceName;

        //Set service endpoint name
        var endpointConfiguration = new EndpointConfiguration(ServiceNames.BillingServiceName);
            
        //Automatically create queues if not exist
        endpointConfiguration.EnableInstallers();

        //Use RabbitMQ as transport
        var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
        transport.UseConventionalRoutingTopology();
        transport.ConnectionString(ConnectionString.RabbitMQConnectionString);
        
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