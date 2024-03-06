using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;

public class Message1Handler :
    IHandleMessages<IMessage1>
{
    static ILog log = LogManager.GetLogger<Message1Handler>();

    public Task Handle(IMessage1 message, IMessageHandlerContext context)
    {
        log.Info($"Received Message1: {message.Property}");

        var message2 = new Message2
        {
            Property = "Hello from Endpoint2"
        };
        return context.Publish(message2);
    }
}