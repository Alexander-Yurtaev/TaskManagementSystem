namespace TMS.TaskService.Extensions.Endpoints.Tasks;

/// <summary>
/// 
/// </summary>
public static class TaskServiceOperations
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="endpoints"></param>
    public static void AddTaskServiceOperations(this IEndpointRouteBuilder endpoints)
    {
        endpoints.AddCreateTaskOperations();
        endpoints.AddReadTaskOperations();
        endpoints.AddUpdateTaskOperations();
        endpoints.AddDeleteTaskOperations();
    }
}