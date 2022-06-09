using DA.Core;
using DA.Core.Command;
using DA.Core.Commands;
using DA.Core.Constants;
using NServiceBus;
using NServiceBus.Logging;

namespace DA.ClientUI;

internal class Program
{
    private static readonly ILog Log = LogManager.GetLogger<Program>();

    private static async Task Main()
    {
        //Init with default config
        var bus = new NBusExtension(ServiceNames.ClientServiceName);
        
        //Enable routing for this endpoint to perform PlaceOrder command towards Sales service
        var routing = bus.Transport.Routing();
        routing.RouteToEndpoint(typeof(PlaceOrder), ServiceNames.SalesServiceName);
        routing.RouteToEndpoint(typeof(CancelOrder), ServiceNames.SalesServiceName);
        
        //Start bus
        var endpointInstance = await Endpoint.Start(bus.EndpointConfiguration).ConfigureAwait(false);

        //Console loop
        await RunLoop(endpointInstance).ConfigureAwait(false);

        //Stop bus
        await endpointInstance.Stop().ConfigureAwait(false);
    }

    private static async Task RunLoop(IEndpointInstance endpointInstance)
    {
        var lastOrder = string.Empty;

        while (true)
        {
            Log.Info("Press 'P' to place an order, 'C' to cancel last order, or 'Q' to quit.");
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
                    Log.Info($"[Order: {command.OrderId}] Sending PlaceOrder command");
                    await endpointInstance.Send(command).ConfigureAwait(false);
                    lastOrder = command.OrderId; // Store order identifier to cancel if needed.
                    break;

                case ConsoleKey.C:
                    var cancelCommand = new CancelOrder
                    {
                        OrderId = lastOrder
                    };
                    await endpointInstance.Send(cancelCommand).ConfigureAwait(false);
                    Log.Info($"[Order: {cancelCommand.OrderId}] Sending CancelOrder command");
                    break;

                case ConsoleKey.Q:
                    return;

                default:
                    Log.Info("Unknown input. Please try again.");
                    break;
            }
        }
    }
}