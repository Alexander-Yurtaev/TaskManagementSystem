using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TMS.Common.Services;

namespace TMS.Common.RabbitMq.Consumers;

public class TaskCreateEventConsumer([FromKeyedServices("EmailEvents")] IFileService fileService, ILogger<TaskCreateEventConsumer> logger)
    : BaseEventConsumer(fileService, logger, CreateQueueName, $"tasks/create/{Guid.NewGuid().ToString()}.txt"),
        IRabbitMqCreateConsumer;