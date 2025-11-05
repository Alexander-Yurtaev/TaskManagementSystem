using Microsoft.Extensions.Logging;

namespace TMS.Common.RabbitMq.Consumers;

public class TaskCreateEventConsumer(ILogger<TaskCreateEventConsumer> logger) 
    : BaseEventConsumer(logger, CreateQueueName, $"tasks/create/{Guid.NewGuid().ToString()}.txt"), IRabbitMqCreateConsumer
{

}