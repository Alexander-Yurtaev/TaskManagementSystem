using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TMS.Common.Services;

namespace TMS.Common.RabbitMq.Consumers;

public class TaskUpdateEventConsumer([FromKeyedServices("EmailEvents")] IFileService fileService, ILogger<TaskUpdateEventConsumer> logger)
    : BaseEventConsumer(fileService, logger, UpdateQueueName, $"tasks/update/{Guid.NewGuid().ToString()}.txt"),
        IRabbitMqUpdateConsumer;