using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace TMS.Common.RabbitMq;

public abstract class BaseRabbitMqService(ILogger<BaseRabbitMqService> logger) : IAsyncDisposable
{
    // Имя exchange
    public const string ExchangeName = "task_events_exchange";

    public const string CreateQueueName = "task_create";
    public const string UpdateQueueName = "task_update";
    public const string DeleteQueueName = "task_delete";
    
    public const string RoutingCreateKey = "task.create";
    public const string RoutingUpdateKey = "task.update";
    public const string RoutingDeleteKey = "task.delete";

    protected IConnection? Connection;
    protected IChannel? Channel;

    // Асинхронная инициализация (вызов при старте приложения)
    public virtual async Task InitializeAsync()
    {
        var rabbitmqHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST");
        if (rabbitmqHost is null)
        {
            var message = "RABBITMQ_HOST does not defined.";
            logger.LogError(message);
            throw new Exception(message);
        }

        var factory = new ConnectionFactory { HostName = rabbitmqHost };

        Connection = await factory.CreateConnectionAsync();
        Channel = await Connection.CreateChannelAsync();

        await Channel.ExchangeDeclareAsync(
            exchange: ExchangeName,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            arguments: null
        );
        
        // Создаем и связываем очереди с exchange по routingKey

        #region Create

        await Channel.QueueDeclareAsync(
            queue: CreateQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        await Channel.QueueBindAsync(
            queue: CreateQueueName,
            exchange: ExchangeName,
            routingKey: RoutingCreateKey,
            arguments: null
        );

        #endregion Create

        #region Update

        await Channel.QueueDeclareAsync(
            queue: UpdateQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        await Channel.QueueBindAsync(
            queue: UpdateQueueName,
            exchange: ExchangeName,
            routingKey: RoutingUpdateKey,
            arguments: null
        );

        #endregion Update

        #region Delete

        await Channel.QueueDeclareAsync(
            queue: DeleteQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        await Channel.QueueBindAsync(
            queue: DeleteQueueName,
            exchange: ExchangeName,
            routingKey: RoutingDeleteKey,
            arguments: null
        );

        #endregion Delete
    }

    public async ValueTask DisposeAsync()
    {
        if (Connection != null) await Connection.DisposeAsync();
        if (Channel != null) await Channel.DisposeAsync();
        Channel?.Dispose();
        Connection?.Dispose();
    }
}