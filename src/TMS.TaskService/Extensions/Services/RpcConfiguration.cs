using TMS.TaskManager.Grpc.Clients;

namespace TMS.TaskService.Extensions.Services;

/// <summary>
/// 
/// </summary>
public static class RpcConfiguration
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <exception cref="Exception"></exception>
    public static void AddRpcConfiguration(this IServiceCollection services, ConfigurationManager configuration)
    {
        var grpcServerSchema = configuration["GRPC_SERVER_SCHEMA"] ?? throw new Exception("GRPC_SERVER_SCHEMA does not defined");
        var grpcServerHost = configuration["GRPC_SERVER_HOST"] ?? throw new Exception("GRPC_SERVER_HOST does not defined");
        var grpcServerPort = configuration["GRPC_SERVER_PORT"] ?? throw new Exception("GRPC_SERVER_PORT does not defined");

        services.AddGrpcClient<Auth.AuthClient>("AuthClient",
            options =>
            {
                options.Address = new Uri($"{grpcServerSchema}://{grpcServerHost}:{grpcServerPort}");
            });
    }
}