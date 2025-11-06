using Microsoft.Extensions.DependencyInjection;
using TMS.Common.Services;

namespace TMS.Common.Extensions;

public static class FileServiceExtensions
{
    public static IServiceCollection AddFileService(this IServiceCollection services,
        string name,
        Action<IFileService> configureService)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureService);

        services.AddKeyedSingleton<IFileService, FileService>(name, (_, _) =>
        {
            var service = new FileService();
            configureService(service);
            return service;
        });

        return services;
    }
}