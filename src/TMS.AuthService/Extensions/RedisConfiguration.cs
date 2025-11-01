using Microsoft.Extensions.Options;
using StackExchange.Redis;
using TMS.AuthService.Configurations;
using TMS.AuthService.Models;
using TMS.AuthService.Services;

namespace TMS.AuthService.Extensions;

/// <summary>
/// 
/// </summary>
public static class RedisConfiguration
{
    /// <summary>
    /// 
    /// </summary>
    public static void AddRedisConfiguration(this IServiceCollection services, ConfigurationManager configuration)
    {
        services.Configure<RedisOptions>(configuration.GetSection("Redis"));
        services.AddSingleton<ConnectionMultiplexer>(provider =>
        {
            var config = provider.GetRequiredService<IOptions<RedisOptions>>().Value;
            return ConnectionMultiplexer.Connect($"{config.Host}:{config.Port}");
        });
        services.AddTransient<IRedisService<UserToken>, RedisService<UserToken>>();
    }
}