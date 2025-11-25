using TMS.Common.Services;
using TMS.FileStorageService.Services;

namespace TMS.FileStorageService.Extensions.Services;

/// <summary>
/// 
/// </summary>
public static class FileServiceExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddFileService(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<IFileStorage, LocalFileStorage>();
        services.AddScoped<IFileService, FileService>();

        return services;
    }
}