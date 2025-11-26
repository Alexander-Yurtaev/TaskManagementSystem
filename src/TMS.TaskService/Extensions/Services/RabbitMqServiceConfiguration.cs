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
        services.AddSingleton<RabbitMqService>();
        services.AddSingleton<IRabbitMqService>(sp => sp.GetRequiredService<RabbitMqService>());
        services.AddSingleton<IRabbitMqServiceInitializable>(sp => sp.GetRequiredService<RabbitMqService>());

        services.AddHostedService<RabbitMqServiceInitializer>();
    }
}