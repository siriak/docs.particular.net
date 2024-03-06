using System;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Features;
using NServiceBus.Pipeline;
using Shared;

class Program
{
    static async Task Main()
    {
        Console.Title = "Samples.ASBS.SendReply.Endpoint2";

        var endpointConfiguration = new EndpointConfiguration("Samples.ASBS.SendReply.Endpoint2");
        endpointConfiguration.EnableInstallers();

        var connectionString = Environment.GetEnvironmentVariable("AzureServiceBus_ConnectionString");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new Exception("Could not read the 'AzureServiceBus_ConnectionString' environment variable. Check the sample prerequisites.");
        }

        var transport = new AzureServiceBusTransport(connectionString);
        endpointConfiguration.UseTransport(transport);
        endpointConfiguration.UseSerialization<SystemJsonSerializer>();

        // temporarily disable auto subscription only for those message types
        var autoSubscribeSettings = endpointConfiguration.AutoSubscribe();
        autoSubscribeSettings.DisableFor<IMessage1>();
        autoSubscribeSettings.DisableFor<IMessage2>();

        endpointConfiguration.Pipeline.Register(new OutgoingBehavior(), "Bla");

        var endpointInstance = await Endpoint.Start(endpointConfiguration)
            .ConfigureAwait(false);

        Console.WriteLine("Press any key to exit");
        Console.ReadKey();

        await endpointInstance.Stop()
            .ConfigureAwait(false);
    }


}