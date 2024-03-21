using System;
using System.Threading.Tasks;
using NServiceBus;

class Program
{
    static async Task Main(string[] args)
    {
        Console.Title = "Receiver";
        var endpointConfiguration = new EndpointConfiguration("Samples.SessionFilter.Receiver");

        endpointConfiguration.UsePersistence<LearningPersistence>();
        endpointConfiguration.UseSerialization<SystemJsonSerializer>();
        endpointConfiguration.UseTransport(new LearningTransport());

        var sessionKeyProvider = new RotatingSessionKeyProvider();

        endpointConfiguration.ApplySessionFilter(sessionKeyProvider);

        var endpointInstance = await Endpoint.Start(endpointConfiguration);

        MainLoop(sessionKeyProvider);

        await endpointInstance.Stop();
    }

    static void MainLoop(RotatingSessionKeyProvider sessionKeyProvider)
    {
        PrintMenu(sessionKeyProvider);

        while (true)
        {

            switch (Console.ReadKey(true).Key)
            {
                case ConsoleKey.C:
                    sessionKeyProvider.NextKey();
                    PrintMenu(sessionKeyProvider);
                    break;
                case ConsoleKey.Escape:
                    return;
            }
        }
    }

    static void PrintMenu(ISessionKeyProvider sessionKeyProvider)
    {
        Console.Clear();
        Console.WriteLine($"Session Key: {sessionKeyProvider.SessionKey}");
        Console.WriteLine("C)   Change Session Key");
        Console.WriteLine("ESC) Close");
    }
}

class SomeMessageHandler : IHandleMessages<SomeMessage>
{
    public Task Handle(SomeMessage message, IMessageHandlerContext context)
    {
        Console.WriteLine($"Got message {message.Counter}");
        return Task.CompletedTask;
    }
}
