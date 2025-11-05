using Microsoft.Extensions.Hosting;

namespace TMS.Common.RabbitMq.Consumers.Initializers;

public class RabbitMqUpdateConsumerInitializer(IRabbitMqUpdateConsumer consumer) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await consumer.InitializeAsync();
    }
}