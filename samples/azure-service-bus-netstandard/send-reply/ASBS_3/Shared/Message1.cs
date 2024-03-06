using NServiceBus;

public class Message1 : IMessage1
{
    public string Property { get; set; }
}

public interface IMessage1 : IEvent
{
    string Property { get; set; }
}