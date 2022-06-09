using DA.Core;
using DA.Core.Constants;
using NServiceBus;

namespace DA.Sales;

internal static class Program
{
    private static async Task Main()
    {
        //Init with default config
        var bus = new NBusExtension(ServiceNames.SalesServiceName);
        
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