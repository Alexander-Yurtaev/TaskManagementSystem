using TMS.Common.Services;
using TMS.NotificationService.Services;

namespace TMS.NotificationService.Extensions.Services;

/// <summary>
/// 
/// </summary>
public static class EmailServiceExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddEmailService(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));

        services.AddScoped<IFileStorage, LocalFileStorage>();
        services.AddScoped<IEmailService, EmailFileService>();

        return services;
    }
}