namespace TMS.Common.RabbitMq;

public interface IRabbitMqServiceInitializable
{
    Task InitializeAsync();
}