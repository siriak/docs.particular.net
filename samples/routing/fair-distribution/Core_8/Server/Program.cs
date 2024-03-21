using System;
using System.Threading.Tasks;
using NServiceBus;

class Program
{
    static async Task Main()
    {
        Console.Title = "Server1";
        var endpointConfiguration = new EndpointConfiguration("Samples.FairDistribution.Server");
        endpointConfiguration.OverrideLocalAddress("Samples.FairDistribution.Server-1");
        endpointConfiguration.UsePersistence<NonDurablePersistence>();
        endpointConfiguration.UseSerialization<SystemJsonSerializer>();
        endpointConfiguration.UseTransport(new MsmqTransport());
        endpointConfiguration.EnableInstallers();
        endpointConfiguration.SendFailedMessagesTo("error");
        endpointConfiguration.LimitMessageProcessingConcurrencyTo(1);
        endpointConfiguration.AuditProcessedMessagesTo("audit");

        #region FairDistributionServer

        endpointConfiguration.EnableFeature<FairDistribution>();

        #endregion

        var endpointInstance = await Endpoint.Start(endpointConfiguration);
        Console.WriteLine("Press any key to exit");
        Console.ReadKey();
        await endpointInstance.Stop();
    }
}
