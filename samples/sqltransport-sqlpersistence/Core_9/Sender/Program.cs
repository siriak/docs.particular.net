using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NServiceBus;
using NServiceBus.Transport.SqlServer;
using Sender;

Console.Title = "Sender";

var endpointConfiguration = new EndpointConfiguration("Samples.Sql.Sender");
endpointConfiguration.SendFailedMessagesTo("error");
endpointConfiguration.EnableInstallers();

#region SenderConfiguration

// for SqlExpress use Data Source=.\SqlExpress;Initial Catalog=NsbSamplesSql;Integrated Security=True;Max Pool Size=100;Encrypt=false
var connectionString = @"Server=localhost,1433;Initial Catalog=NsbSamplesSql;User Id=SA;Password=yourStrong(!)Password;Max Pool Size=100;Encrypt=false";
var transport = new SqlServerTransport(connectionString)
{
    DefaultSchema = "sender",
    Subscriptions =
            {
                SubscriptionTableName = new SubscriptionTableName(
                        table: "Subscriptions",
                        schema: "dbo")
            },
    TransportTransactionMode = TransportTransactionMode.SendsAtomicWithReceive
};
transport.SchemaAndCatalog.UseSchemaForQueue("error", "dbo");
transport.SchemaAndCatalog.UseSchemaForQueue("audit", "dbo");

endpointConfiguration.UseSerialization<SystemJsonSerializer>();
endpointConfiguration.UseTransport(transport);

#endregion

await SqlHelper.CreateSchema(connectionString, "sender");

var hostBuilder = Host.CreateApplicationBuilder();
hostBuilder.UseNServiceBus(endpointConfiguration);
hostBuilder.Services.AddHostedService<Worker>();
var host = hostBuilder.Build();
await host.RunAsync();