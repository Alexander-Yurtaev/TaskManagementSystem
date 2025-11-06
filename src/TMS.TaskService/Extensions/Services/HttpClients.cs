namespace TMS.TaskService.Extensions.Services;

/// <summary>
///
/// </summary>
public static class HttpClients
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="services"></param>
    public static void AddFileStorageClient(this IServiceCollection services)
    {
        services.AddHttpClient(name: "TMS.FileStorageClient",
            configureClient: options =>
            {
                var filesHost = Environment.GetEnvironmentVariable("FILES_HOST");
                if (string.IsNullOrEmpty(filesHost))
                {
                    throw new InvalidOperationException("FILES_HOST is not defined.");
                }

                options.BaseAddress = new Uri($"https://{filesHost}:8081/files");
            });
    }
}