using StackExchange.Redis;
using TMS.AuthService.Models;
using TMS.AuthService.Services;

namespace TMS.AuthService.Extensions.Services;

/// <summary>
/// 
/// </summary>
public static class RedisConfiguration
{
    /// <summary>
    /// 
    /// </summary>
    public static ConnectionMultiplexer AddRedisConfiguration(this IServiceCollection services, ConfigurationManager configuration)
    {
        var connectionMultiplexer = GetConnectionMultiplexer(configuration);
        services.AddSingleton(_ => connectionMultiplexer);
        services.AddTransient<IRedisService<UserToken>, RedisService<UserToken>>();

        return connectionMultiplexer;
    }

    #region Static Methods

    private static ConnectionMultiplexer GetConnectionMultiplexer(ConfigurationManager configuration)
    {
        var host = configuration["REDIS_HOST"];
        var port = configuration["REDIS_PORT"];
        var password = configuration["REDIS_PASSWORD"];

        // Создаем конфигурацию с паролем
        var redisConfig = new ConfigurationOptions
        {
            EndPoints = { $"{host}:{port}" },
            Password = password,
            ConnectTimeout = 5000,
            SyncTimeout = 5000
        };

        return ConnectionMultiplexer.Connect(redisConfig);
    }

    #endregion Static Methods
}