using System;
using NServiceBus;
using System.Threading.Tasks;
using Endpoint.SqlPersistence.Miscellaneous;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class SubmitOrderHandler :
    IHandleMessages<SubmitOrder>
{
    ReceiverDataContext receiverDataContext;
    private readonly ShipmentDataContext shipmentDataContext;
    private readonly ApplicationDbContext applicationDbContext;
    private readonly UserManager<ApplicationUser> userManager;
    private readonly ILogger<SubmitOrderHandler> logger;

    public SubmitOrderHandler(ReceiverDataContext receiverDataContext, ShipmentDataContext shipmentDataContext,
        ApplicationDbContext applicationDbContext, UserManager<ApplicationUser> userManager,
        ILogger<SubmitOrderHandler> logger)
    {
        this.receiverDataContext = receiverDataContext;
        this.shipmentDataContext = shipmentDataContext;
        this.applicationDbContext = applicationDbContext;
        this.userManager = userManager;
        this.logger = logger;
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

        await userManager.CreateAsync(new ApplicationUser
        {
            UserName = order.OrderId
        });

        logger.LogInformation($"Order {message.OrderId} worth {message.Value} created.");

        await context.SendLocal(new CompleteOrder { OrderId = message.OrderId });
        await context.SendLocal(new CompleteOrder { OrderId = message.OrderId });
    }
}