namespace TMS.TaskService.Extensions.Endpoints.Tasks;

/// <summary>
/// 
/// </summary>
public static class TasksOperations
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="endpoints"></param>
    public static void AddTasksOperations(this IEndpointRouteBuilder endpoints)
    {
        endpoints.AddCreateTaskOperations();
        endpoints.AddReadTaskOperations();
        endpoints.AddUpdateTaskOperations();
        endpoints.AddDeleteTaskOperations();
    }
}