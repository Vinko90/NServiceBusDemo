using System.Runtime.InteropServices;
using DA.Core.Constants;
using Microsoft.Data.SqlClient;
using NServiceBus;

namespace DA.Core;

public class NBusExtension
{    
    public EndpointConfiguration EndpointConfiguration { get; private set; }
    
    public TransportExtensions<RabbitMQTransport> Transport { get; private set; }
    
    private string ServiceName { get; }
    
    public NBusExtension(string serviceName)
    {
        ServiceName = serviceName;
        Console.Title = serviceName;
        ConfigureEndpoint();
    }

    public void ConfigurePersistence()
    {
        //Test
        //Add Saga persistence to MongoDB
        //var persistence = endpointConfiguration.UsePersistence<MongoPersistence>();
        //persistence.MongoClient(new MongoClient(ConnectionString.MongoDBConnectionString));
        //persistence.DatabaseName(ConnectionString.MongoDatabaseName);
        //persistence.UseTransactions(false);
        
        //Add Saga persistence to MSSQL
        var persistence = EndpointConfiguration.UsePersistence<SqlPersistence>();
        var subscriptions = persistence.SubscriptionSettings();
        subscriptions.CacheFor(TimeSpan.FromMinutes(1));
        persistence.SqlDialect<SqlDialect.MsSqlServer>();
        persistence.ConnectionBuilder(connectionBuilder: () => new SqlConnection(ConnectionString.MSSQLConnectionString));
    }
    
    private async void ConfigureEndpoint()
    {
        //Set service endpoint name
        EndpointConfiguration = new EndpointConfiguration(ServiceName);
        
        //Automatically create queues if not exist
        EndpointConfiguration.EnableInstallers();
        
        //Use RabbitMQ as transport
        Transport = EndpointConfiguration.UseTransport<RabbitMQTransport>();
        Transport.UseConventionalRoutingTopology();
        Transport.ConnectionString(ConnectionString.RabbitMQConnectionString);
        
        //In OSX we don't have ServiceControl
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) return;
        //Configure monitoring system
        var json = await File.ReadAllTextAsync("ServicePulseConfig.json");
        var servicePlatformConnection = ServicePlatformConnectionConfiguration.Parse(json);
        EndpointConfiguration.ConnectToServicePlatform(servicePlatformConnection);
    }
}