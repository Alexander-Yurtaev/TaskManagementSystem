using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Threading.Tasks;
using TMS.Common.Helpers;
using TMS.Common.Validators;

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
        var result = RabbitMQHelper.CreateConnectionFactory(logger);

        if (!result.Item1.IsValid)
        {
            await Task.FromException(new Exception(result.Item1.ErrorMessage));
        }

        Connection = await result.Item2!.CreateConnectionAsync();
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
    }
}