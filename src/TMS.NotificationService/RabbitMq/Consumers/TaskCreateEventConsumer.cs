namespace TMS.NotificationService.RabbitMq.Consumers;

/// <summary>
/// 
/// </summary>
/// <param name="serviceScopeFactory"></param>
/// <param name="logger"></param>
public class TaskCreateEventConsumer(IServiceScopeFactory serviceScopeFactory, ILogger<TaskCreateEventConsumer> logger)
    : BaseEventConsumer(serviceScopeFactory, logger, CreateQueueName, $"tasks/create"),
        IRabbitMqCreateConsumer;