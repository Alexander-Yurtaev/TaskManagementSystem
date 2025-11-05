using Microsoft.Extensions.Logging;

namespace TMS.Common.RabbitMq.Consumers;

public class TaskUpdateEventConsumer(ILogger<TaskUpdateEventConsumer> logger)
    : BaseEventConsumer(logger, UpdateQueueName, $"tasks/update/{Guid.NewGuid().ToString()}.txt"), IRabbitMqUpdateConsumer
{

}