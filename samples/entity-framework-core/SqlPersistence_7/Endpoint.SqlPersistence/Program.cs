using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NServiceBus;
using NServiceBus.Persistence.Sql;
using System;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        // for SqlExpress use Data Source=.\SqlExpress;Initial Catalog=NsbSamplesEfCoreUowSql;Integrated Security=True;Encrypt=false
        var sqlConnectionString = @"Server=localhost,1433;Initial Catalog=NsbSamplesEfCoreUowSql;User Id=SA;Password=yourStrong(!)Password;Encrypt=false;Max Pool Size=100";
        var databaseConnectionString = @"Server=localhost,1433;Initial Catalog=NsbSamplesEfCoreUowSqlPersistence;User Id=SA;Password=yourStrong(!)Password;Encrypt=false;Max Pool Size=100";
        Console.Title = "Samples.EntityFrameworkUnitOfWork.SQL";

        await SqlHelper.EnsureDatabaseExists(databaseConnectionString);

        await using (var receiverDataContext = new ReceiverDataContext(new DbContextOptionsBuilder<ReceiverDataContext>()
                         .UseSqlServer(sqlConnectionString)
                         .Options))
        {
            await receiverDataContext.Database.EnsureCreatedAsync().ConfigureAwait(false);
        }

        var endpointConfiguration = new EndpointConfiguration("Samples.EntityFrameworkUnitOfWork.SQL");
        endpointConfiguration.EnableInstallers();
        endpointConfiguration.SendFailedMessagesTo("error");

        endpointConfiguration.UseTransport(new AzureServiceBusTransport(Environment.GetEnvironmentVariable("AZURE_SERVICEBUS_CONNECTION_STRING")));

        endpointConfiguration.UseSerialization<NewtonsoftJsonSerializer>();

        var persistence = endpointConfiguration.UsePersistence<SqlPersistence>();
        persistence.ConnectionBuilder(() => new SqlConnection(databaseConnectionString));
        var dialect = persistence.SqlDialect<SqlDialect.MsSqlServer>();

        endpointConfiguration.RegisterComponents(services =>
        {
            services.AddDbContext<ReceiverDataContext>(config => config.UseSqlServer(sqlConnectionString));
            services.AddDbContext<ShipmentDataContext>(config => config.UseSqlServer(sqlConnectionString));
        });

        var endpointInstance = await Endpoint.Start(endpointConfiguration)
            .ConfigureAwait(false);

        await Sender.Start(endpointInstance);
    }
}
