using System;
using System.Linq;
using NServiceBus;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NServiceBus.Logging;

public class SubmitOrderHandler :
    IHandleMessages<SubmitOrder>
{
    static ILog log = LogManager.GetLogger<SubmitOrderHandler>();
    ReceiverDataContext receiverDataContext;
    private readonly ShipmentDataContext shipmentDataContext;

    public SubmitOrderHandler(ReceiverDataContext receiverDataContext, ShipmentDataContext shipmentDataContext)
    {
        this.receiverDataContext = receiverDataContext;
        this.shipmentDataContext = shipmentDataContext;
    }

    public async Task Handle(SubmitOrder message, IMessageHandlerContext context)
    {
        var order = new Order
        {
            OrderId = message.OrderId,
            Value = message.Value
        };
        receiverDataContext.Orders.Add(order);
        await receiverDataContext.SaveChangesAsync(context.CancellationToken);

        var loadedOrder = await shipmentDataContext.Orders
            .SingleAsync(x => x.OrderId == message.OrderId, context.CancellationToken);

        var shipment = new Shipment
        {
            Id = Guid.NewGuid(),
            Order = loadedOrder,
            Location = message.ShipTo
        };
        shipmentDataContext.Shipments.Add(shipment);
        await shipmentDataContext.SaveChangesAsync(context.CancellationToken);

        log.Info($"Order {message.OrderId} worth {message.Value} created.");

        await context.SendLocal(new CompleteOrder { OrderId = message.OrderId });
        await context.SendLocal(new CompleteOrder { OrderId = message.OrderId });
    }
}
