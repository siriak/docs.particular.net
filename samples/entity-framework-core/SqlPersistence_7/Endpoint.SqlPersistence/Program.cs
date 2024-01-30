using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NServiceBus;
using System;
using Endpoint.SqlPersistence.Miscellaneous;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// for SqlExpress use Data Source=.\SqlExpress;Initial Catalog=NsbSamplesEfCoreUowSql;Integrated Security=True;Encrypt=false
var sqlConnectionString =
    "Server=localhost,1433;Initial Catalog=NsbSamplesEfCoreUowSql;User Id=SA;Password=yourStrong(!)Password;Encrypt=false;Max Pool Size=100";
var databaseConnectionString =
    "Server=localhost,1433;Initial Catalog=NsbSamplesEfCoreUowSqlPersistence;User Id=SA;Password=yourStrong(!)Password;Encrypt=false;Max Pool Size=100";
Console.Title = "Samples.EntityFrameworkUnitOfWork.SQL";

await SqlHelper.EnsureDatabaseExists(databaseConnectionString);
await SqlHelper.EnsureDatabaseExists(sqlConnectionString);

await using (var shipmentDataContext = new ShipmentDataContext(new DbContextOptionsBuilder<ShipmentDataContext>()
                 .UseSqlServer(sqlConnectionString)
                 .Options))
{
    await shipmentDataContext.Database.EnsureCreatedAsync().ConfigureAwait(false);
}

await using (var receiverDataContext = new ReceiverDataContext(new DbContextOptionsBuilder<ReceiverDataContext>()
                 .UseSqlServer(sqlConnectionString)
                 .Options))
{
    await receiverDataContext.Database.EnsureCreatedAsync().ConfigureAwait(false);
}

await using (var applicationDbContext = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
                 .UseSqlServer(sqlConnectionString)
                 .Options))
{
    await applicationDbContext.Database.EnsureCreatedAsync().ConfigureAwait(false);
}

var hostBuilder = new HostBuilder();
hostBuilder.ConfigureLogging(logging => logging.AddConsole());
hostBuilder.UseConsoleLifetime();
hostBuilder.UseNServiceBus(_ =>
{
    var endpointConfiguration = new EndpointConfiguration("Samples.EntityFrameworkUnitOfWork.SQL");
    endpointConfiguration.EnableInstallers();
    endpointConfiguration.SendFailedMessagesTo("error");

    endpointConfiguration.UseTransport(
        new AzureServiceBusTransport(Environment.GetEnvironmentVariable("AZURE_SERVICEBUS_CONNECTION_STRING"))
        {
            TransportTransactionMode = TransportTransactionMode.SendsAtomicWithReceive
        });

    endpointConfiguration.UseSerialization<NewtonsoftJsonSerializer>();

    var persistence = endpointConfiguration.UsePersistence<SqlPersistence>();
    persistence.ConnectionBuilder(() => new SqlConnection(databaseConnectionString));
    var dialect = persistence.SqlDialect<SqlDialect.MsSqlServer>();

    endpointConfiguration.RegisterComponents(services =>
    {
        services.AddDbContext<ReceiverDataContext>(config => config.UseSqlServer(sqlConnectionString));
        services.AddDbContext<ShipmentDataContext>(config => config.UseSqlServer(sqlConnectionString));
    });
    return endpointConfiguration;
});
hostBuilder.ConfigureServices(services =>
{
    services
        .AddIdentityCore<ApplicationUser>()
        .AddEntityFrameworkStores<ApplicationDbContext>();
    services.AddDbContext<ApplicationDbContext>(config => config.UseSqlServer(sqlConnectionString));

    services.AddHostedService<Sender>();
});

var host = hostBuilder.Build();
await host.RunAsync();