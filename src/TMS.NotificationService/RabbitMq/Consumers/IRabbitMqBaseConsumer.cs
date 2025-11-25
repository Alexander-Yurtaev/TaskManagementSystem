namespace TMS.NotificationService.RabbitMq.Consumers;

/// <summary>
/// 
/// </summary>
public interface IRabbitMqBaseConsumer
{
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    Task<string> InitializeAsync();
}