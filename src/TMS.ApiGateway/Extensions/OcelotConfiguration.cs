using Ocelot.DependencyInjection;

namespace TMS.ApiGateway.Extensions;

public static class OcelotConfiguration
{
    public static void AddOcelotConfiguration(this IServiceCollection services, 
        string contentRootPath, ConfigurationManager configuration)
    {
        configuration
            .SetBasePath(contentRootPath)
            .AddOcelot(); // single ocelot.json file in read-only mode

        services
            .AddOcelot(configuration);
    }
}