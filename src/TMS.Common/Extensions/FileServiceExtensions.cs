using Microsoft.Extensions.DependencyInjection;
using TMS.Common.Services;

namespace TMS.Common.Extensions;

public class FileServiceOptions
{
    public string BasePath { get; set; } = string.Empty;
}

public static class FileServiceExtensions
{
    public static IServiceCollection AddFileService(this IServiceCollection services,
        string name,
        Action<FileServiceOptions> configureService)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureService);

        services.AddKeyedSingleton<IFileService, FileService>(name, (_, _) =>
        {
            var options = new FileServiceOptions();
            configureService(options);
            var service = new FileService(options);

            return service;
        });

        return services;
    }
}