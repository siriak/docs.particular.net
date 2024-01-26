using NServiceBus;

public class SubmitOrder :
    IMessage
{
    public string OrderId { get; set; }
    public decimal Value { get; set; }
    public string ShipTo { get; set; }
}