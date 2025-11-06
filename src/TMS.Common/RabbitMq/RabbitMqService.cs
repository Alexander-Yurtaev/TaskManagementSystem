using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Text;

namespace TMS.Common.RabbitMq;

public class RabbitMqService(ILogger<RabbitMqService> logger) : BaseRabbitMqService(logger), IRabbitMqService
{
    public async Task SendMessageAsync(TaskMessage message)
    {
        var props = new BasicProperties();
        var body = Encoding.UTF8.GetBytes(message.Message);
        string routingKey;

        switch (message.Type)
        {
            case TaskMessageType.Create:
                routingKey = RoutingCreateKey;
                break;

            case TaskMessageType.Update:
                routingKey = RoutingUpdateKey;
                break;

            case TaskMessageType.Delete:
                routingKey = RoutingDeleteKey;
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        await Channel!.BasicPublishAsync(
            exchange: ExchangeName,
            routingKey: routingKey,
            mandatory: false,
            basicProperties: props,
            body: body
        );
    }
}