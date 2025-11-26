namespace TMS.NotificationService.RabbitMq.Consumers;

/// <summary>
/// 
/// </summary>
/// <param name="serviceScopeFactory"></param>
/// <param name="logger"></param>
public class TaskDeleteEventConsumer(IServiceScopeFactory serviceScopeFactory, ILogger<TaskDeleteEventConsumer> logger)
    : BaseEventConsumer(serviceScopeFactory, logger, DeleteQueueName, $"tasks/delete"),
        IRabbitMqDeleteConsumer;