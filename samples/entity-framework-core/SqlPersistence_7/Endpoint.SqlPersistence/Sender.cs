using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NServiceBus;

public class Sender : BackgroundService
{
    private readonly IMessageSession messageSession;
    private readonly ILogger<Sender> logger;

    public Sender(IMessageSession messageSession, ILogger<Sender> logger)
    {
        this.messageSession = messageSession;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        const string letters = "ABCDEFGHIJKLMNOPQRSTUVXYZ";
        var locations = new[] { "London", "Paris", "Oslo", "Madrid" };

        while (!stoppingToken.IsCancellationRequested)
        {
            var orderId = new string(Enumerable.Range(0, 4).Select(x => letters[Random.Shared.Next(letters.Length)]).ToArray());
            var shipTo = locations[Random.Shared.Next(locations.Length)];
            var orderSubmitted = new SubmitOrder
            {
                OrderId = orderId,
                Value = Random.Shared.Next(100),
                ShipTo = shipTo
            };
            await messageSession.SendLocal(orderSubmitted, cancellationToken: stoppingToken)
                .ConfigureAwait(false);

            logger.LogInformation($"Sent a new SubmitOrder message with OrderId={orderId}.");

            await Task.Delay(1000, stoppingToken);
        }
    }
}
