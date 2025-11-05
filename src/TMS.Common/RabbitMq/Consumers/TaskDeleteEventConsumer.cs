using Microsoft.Extensions.Logging;

namespace TMS.Common.RabbitMq.Consumers;

public class TaskDeleteEventConsumer(ILogger<TaskDeleteEventConsumer> logger)
    : BaseEventConsumer(logger, DeleteQueueName, $"tasks/delete/{Guid.NewGuid().ToString()}.txt"), IRabbitMqDeleteConsumer
{

}