namespace TMS.Common.RabbitMq.Consumers;

public interface IRabbitMqBaseConsumer
{
    Task<string> InitializeAsync();
}