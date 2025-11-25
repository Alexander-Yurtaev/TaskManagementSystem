using TMS.Common.RabbitMq;

namespace TMS.TaskService.Extensions.Services;

/// <summary>
/// 
/// </summary>
public static class RabbitMqServiceConfiguration
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    public static void AddRabbitMqServiceConfiguration(this IServiceCollection services)
    {
        services.AddSingleton<IRabbitMqService, RabbitMqService>();
        services.AddSingleton<IRabbitMqServiceInitializable, RabbitMqService>();

        services.AddHostedService<RabbitMqServiceInitializer>();
    }
}