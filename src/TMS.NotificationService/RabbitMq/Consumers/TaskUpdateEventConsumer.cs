using TMS.NotificationService.RabbitMq.Consumers;

namespace TMS.Common.RabbitMq.Consumers;

/// <summary>
/// 
/// </summary>
/// <param name="serviceScopeFactory"></param>
/// <param name="logger"></param>
public class TaskUpdateEventConsumer(IServiceScopeFactory serviceScopeFactory, ILogger<TaskUpdateEventConsumer> logger)
    : BaseEventConsumer(serviceScopeFactory, logger, UpdateQueueName, $"tasks/update"),
        IRabbitMqUpdateConsumer;