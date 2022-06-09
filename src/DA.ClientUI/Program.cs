using System.Runtime.InteropServices;
using DA.Core.Command;
using DA.Core.Commands;
using DA.Core.Constants;
using NServiceBus;
using NServiceBus.Logging;

namespace DA.ClientUI;

class Program
{
    static ILog log = LogManager.GetLogger<Program>();

    static async Task Main()
    {
        Console.Title = ServiceNames.ClientServiceName;

        //Set service endpoint name
        var endpointConfiguration = new EndpointConfiguration(ServiceNames.ClientServiceName);

        //Automatically create queues if not exist
        endpointConfiguration.EnableInstallers();

        //Use RabbitMQ as transport
        var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
        transport.UseConventionalRoutingTopology();
        transport.ConnectionString(ConnectionString.RabbitMQConnectionString);
            
        //Enable routing to perform PlaceOrder command towards DA.Sales service
        var routing = transport.Routing();
        routing.RouteToEndpoint(typeof(PlaceOrder), ServiceNames.SalesServiceName);
        routing.RouteToEndpoint(typeof(CancelOrder), ServiceNames.SalesServiceName);

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

        //Console loop
        await RunLoop(endpointInstance).ConfigureAwait(false);

        //Stop bus
        await endpointInstance.Stop().ConfigureAwait(false);
    }

    static async Task RunLoop(IEndpointInstance endpointInstance)
    {
        string lastOrder = string.Empty;

        while (true)
        {
            log.Info("Press 'P' to place an order, 'C' to cancel last order, or 'Q' to quit.");
            var key = Console.ReadKey();
            Console.WriteLine();

            switch (key.Key)
            {
                case ConsoleKey.P:
                    // Instantiate the command
                    var command = new PlaceOrder
                    {
                        OrderId = Guid.NewGuid().ToString()
                    };

                    // Send the command
                    log.Info($"[Order: {command.OrderId}] Sending PlaceOrder command");
                        
                    await endpointInstance.Send(command)
                        .ConfigureAwait(false);

                    lastOrder = command.OrderId; // Store order identifier to cancel if needed.
                    break;

                case ConsoleKey.C:
                    var cancelCommand = new CancelOrder
                    {
                        OrderId = lastOrder
                    };
                    await endpointInstance.Send(cancelCommand)
                        .ConfigureAwait(false);
                    log.Info($"[Order: {cancelCommand.OrderId}] Sending CancelOrder command");
                    break;

                case ConsoleKey.Q:
                    return;

                default:
                    log.Info("Unknown input. Please try again.");
                    break;
            }
        }
    }
}