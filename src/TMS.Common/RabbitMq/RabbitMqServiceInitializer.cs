using Microsoft.Extensions.Hosting;

namespace TMS.Common.RabbitMq;

public class RabbitMqServiceInitializer(IRabbitMqService rabbitMqService) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await ((RabbitMqService)rabbitMqService).InitializeAsync();
    }
}