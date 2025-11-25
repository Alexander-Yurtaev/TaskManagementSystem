namespace TMS.NotificationService.RabbitMq.Consumers.Initializers;

/// <summary>
/// 
/// </summary>
/// <param name="consumer"></param>
public class RabbitMqCreateConsumerInitializer(IRabbitMqCreateConsumer consumer) : BackgroundService
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await consumer.InitializeAsync();
    }
}