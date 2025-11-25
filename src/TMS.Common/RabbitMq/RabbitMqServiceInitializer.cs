using Microsoft.Extensions.Hosting;

namespace TMS.Common.RabbitMq;

public class RabbitMqServiceInitializer(IRabbitMqServiceInitializable rabbitMqService) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await rabbitMqService.InitializeAsync();
    }
}