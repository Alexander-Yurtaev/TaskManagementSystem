using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace TMS.TaskService.Data.Extensions;

public static class DataContextExtensions
{
    public static IServiceCollection AddTaskDataContext(this IServiceCollection services,
        string? connectionString = null)
    {
        if (connectionString is null)
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

            connectionString = builder.ConnectionString;
        }

        services.AddDbContext<TaskDataContext>(options =>
            {
                options.UseNpgsql(connectionString);
            },
            contextLifetime: ServiceLifetime.Transient,
            optionsLifetime: ServiceLifetime.Transient);

        return services;
    }
}