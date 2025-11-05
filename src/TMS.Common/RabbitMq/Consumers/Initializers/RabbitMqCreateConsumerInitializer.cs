using Microsoft.Extensions.Hosting;

namespace TMS.Common.RabbitMq.Consumers.Initializers;

public class RabbitMqCreateConsumerInitializer(IRabbitMqCreateConsumer consumer) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await consumer.InitializeAsync();
    }
}