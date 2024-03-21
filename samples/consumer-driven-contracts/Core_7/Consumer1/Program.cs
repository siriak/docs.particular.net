﻿using System;
using System.Threading.Tasks;
using NServiceBus;

class Program
{
    static async Task Main()
    {
        Console.Title = "Consumer1";
        var endpointConfiguration = new EndpointConfiguration("Samples.ConsumerDrivenContracts.Consumer1");
        var transport = endpointConfiguration.UseTransport<LearningTransport>();
        endpointConfiguration.UseSerialization<NewtonsoftJsonSerializer>();

        endpointConfiguration.SendFailedMessagesTo("error");
        endpointConfiguration.EnableInstallers();

        var endpointInstance = await Endpoint.Start(endpointConfiguration);

        Console.WriteLine("Press any key to exit");

        Console.ReadKey();

        await endpointInstance.Stop();
    }
}