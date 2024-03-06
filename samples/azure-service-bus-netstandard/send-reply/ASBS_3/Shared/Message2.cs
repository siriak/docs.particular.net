using NServiceBus;

public class Message2 :
    IMessage2
{
    public string Property { get; set; }
}

public interface IMessage2 : IEvent
{
    string Property { get; set; }
}