using DA.Core;
using DA.Core.Commands;
using DA.Core.Constants;
using NServiceBus;

namespace DA.Shipping;

internal static class Program
{
    private static async Task Main()
    {
        //Init with default config
        var bus = new NBusExtension(ServiceNames.ShippingServiceName);
        
        //Routing Configuration
        var routing = bus.Transport.Routing();
        routing.RouteToEndpoint(typeof(ShipOrder), ServiceNames.ShippingServiceName);
        routing.RouteToEndpoint(typeof(ShipWithMaple), ServiceNames.ShippingServiceName);
        routing.RouteToEndpoint(typeof(ShipWithAlpine), ServiceNames.ShippingServiceName);

        //Enable Saga Persistence
        bus.ConfigurePersistence();
        
        //Start bus
        var endpointInstance = await Endpoint.Start(bus.EndpointConfiguration).ConfigureAwait(false);

        Console.WriteLine("Press Enter to exit.");
        Console.ReadLine();

        //Stop bus
        await endpointInstance.Stop().ConfigureAwait(false);
    }
}