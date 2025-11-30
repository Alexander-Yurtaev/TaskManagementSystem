using Microsoft.EntityFrameworkCore;
using TMS.Common.Helpers;

namespace TMS.TaskService.Data.Extensions;

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
    public static IServiceCollection AddTaskDataContext(this IServiceCollection services,
        string? connectionString = null)
    {
        if (connectionString is null)
        {
            connectionString = PostgresHelper.GetConnectionString();
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