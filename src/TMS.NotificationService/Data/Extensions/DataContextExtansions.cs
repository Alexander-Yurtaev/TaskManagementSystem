using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace TMS.NotificationService.Data.Extensions;

/// <summary>
/// 
/// </summary>
public static class DataContextExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    /// <param name="connectionString"></param>
    /// <returns></returns>
    public static IServiceCollection AddNotifyDataContext(this IServiceCollection services,
        string? connectionString = null)
    {
        if (connectionString is null)
        {
            connectionString = GetConnectionString();
        }

        services.AddDbContext<NotificationDataContext>(options =>
            {
                options.UseNpgsql(connectionString);
            },
            contextLifetime: ServiceLifetime.Transient,
            optionsLifetime: ServiceLifetime.Transient);

        return services;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static string? GetConnectionString()
    {
        NpgsqlConnectionStringBuilder builder = new()
        {
            Host = Environment.GetEnvironmentVariable("POSTGRES_HOST"),
            Port = 5432, // порт по умолчанию для PostgreSQL
            Database = Environment.GetEnvironmentVariable("POSTGRES_DB"),
            Username = Environment.GetEnvironmentVariable("POSTGRES_USER"),
            Password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD"),
            CommandTimeout = 30 // таймаут выполнения команд
        };

        return builder.ConnectionString;
    }
}