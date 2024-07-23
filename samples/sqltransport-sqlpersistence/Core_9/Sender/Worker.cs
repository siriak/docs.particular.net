using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using NServiceBus;

namespace Sender
{
    class Worker(IMessageSession messageSession, IHostApplicationLifetime applicationLifetime) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Press enter to send a message");
            Console.WriteLine("Press any other key to exit");

            while (!cancellationToken.IsCancellationRequested)
            {
                if (!Console.KeyAvailable)
                {
                    await Task.Delay(100, cancellationToken);
                    continue;
                }
                var key = Console.ReadKey(true).Key;
                Console.WriteLine();

                if (key != ConsoleKey.Enter)
                {
                    break;
                }

                var orderSubmitted = new OrderSubmitted
                {
                    OrderId = Guid.NewGuid(),
                    Value = Random.Shared.Next(100)
                };
                await messageSession.Publish(orderSubmitted);
                Console.WriteLine("Published OrderSubmitted message");
            }

            applicationLifetime.StopApplication();
        }
    }
}
