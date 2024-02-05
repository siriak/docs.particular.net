using System;
using NServiceBus;

Console.Title = "Receiver";
var endpointConfiguration = new EndpointConfiguration("FixMalformedMessages.Receiver");
endpointConfiguration.UseSerialization<SystemJsonSerializer>();
endpointConfiguration.UseTransport<LearningTransport>();

#region DisableRetries

var recoverability = endpointConfiguration.Recoverability();

recoverability.Delayed(
    customizations: retriesSettings =>
    {
        retriesSettings.NumberOfRetries(0);
    });
recoverability.Immediate(
    customizations: retriesSettings =>
    {
        retriesSettings.NumberOfRetries(0);
    });

#endregion

var endpointInstance = await Endpoint.Start(endpointConfiguration);

Console.WriteLine("Press 'Enter' to finish.");
Console.ReadLine();

await endpointInstance.Stop();