using TMS.Common.RabbitMq.Consumers;
using TMS.Common.RabbitMq.Consumers.Initializers;

namespace TMS.NotificationService.Extensions.Services;

public static class RabbitMqConsumerConfiguration
{
    public static void AddRabbitMqConsumerConfiguration(this IServiceCollection services)
    {
        services.AddSingleton<IRabbitMqCreateConsumer, TaskCreateEventConsumer>();
        services.AddSingleton<IRabbitMqUpdateConsumer, TaskUpdateEventConsumer>();
        services.AddSingleton<IRabbitMqDeleteConsumer, TaskDeleteEventConsumer>();

        services.AddHostedService<RabbitMqCreateConsumerInitializer>();
        services.AddHostedService<RabbitMqUpdateConsumerInitializer>();
        services.AddHostedService<RabbitMqDeleteConsumerInitializer>();
    }
}