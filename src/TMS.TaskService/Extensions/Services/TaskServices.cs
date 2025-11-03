using TMS.TaskService.Data.Repositories;

namespace TMS.TaskService.Extensions.Services;

/// <summary>
/// 
/// </summary>
public static class TaskServices
{
    /// <summary>
    /// 
    /// </summary>
    public static void AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
    }
}