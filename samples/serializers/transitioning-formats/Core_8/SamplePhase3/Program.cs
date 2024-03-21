﻿using System;
using Newtonsoft.Json;
using NServiceBus;

Console.Title = "TransitionPhase3";
var endpointConfiguration = new EndpointConfiguration("Samples.Serialization.TransitionPhase3");
endpointConfiguration.SharedConfig();

#region Phase3

var settingsV2 = new JsonSerializerSettings
{
    Formatting = Formatting.Indented,
    ContractResolver = new ExtendedResolver()
};

var serializationV2 = endpointConfiguration.UseSerialization<NewtonsoftJsonSerializer>();
serializationV2.Settings(settingsV2);
serializationV2.ContentTypeKey("jsonv2");

var settingsV1 = new JsonSerializerSettings
{
    Formatting = Formatting.Indented
};

var serializationV1 = endpointConfiguration.AddDeserializer<NewtonsoftJsonSerializer>();
serializationV1.Settings(settingsV1);
serializationV1.ContentTypeKey("jsonv1");

#endregion

var endpointInstance = await Endpoint.Start(endpointConfiguration);

#region send-to-both
var message = MessageCreator.NewOrder();

await endpointInstance.SendLocal(message);

await endpointInstance.Send("Samples.Serialization.TransitionPhase2", message);

await endpointInstance.Send("Samples.Serialization.TransitionPhase4", message);
#endregion

Console.WriteLine("Order Sent");
Console.WriteLine("Press any key to exit");
Console.ReadKey();

await endpointInstance.Stop();
