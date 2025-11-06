using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TMS.Common.Services;

namespace TMS.Common.RabbitMq.Consumers;

public class TaskDeleteEventConsumer([FromKeyedServices("EmailEvents")] IFileService fileService, ILogger<TaskDeleteEventConsumer> logger)
    : BaseEventConsumer(fileService, logger, DeleteQueueName, $"tasks/delete/{Guid.NewGuid().ToString()}.txt"),
        IRabbitMqDeleteConsumer;