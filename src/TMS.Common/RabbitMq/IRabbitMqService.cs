namespace TMS.Common.RabbitMq;

public interface IRabbitMqService
{
    Task SendMessageAsync(TaskMessage message);
}