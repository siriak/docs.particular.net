using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NServiceBus;

public class CompleteOrderHandler
    : IHandleMessages<CompleteOrder>
{
    ReceiverDataContext dataContext;
    private readonly ILogger<CompleteOrder> logger;

    public CompleteOrderHandler(ReceiverDataContext dataContext, ILogger<CompleteOrder> logger)
    {
        this.dataContext = dataContext;
        this.logger = logger;
    }

    public async Task Handle(CompleteOrder message, IMessageHandlerContext context)
    {
        var order = await dataContext.Orders.FindAsync(message.OrderId, context.CancellationToken)
            .ConfigureAwait(false);
        var shipment = await dataContext.Shipments.FirstAsync(x => x.Order.OrderId == order.OrderId, context.CancellationToken)
            .ConfigureAwait(false);

        logger.LogInformation($"Completing order {order.OrderId} worth {order.Value} by shipping to {shipment.Location}.");
    }
}