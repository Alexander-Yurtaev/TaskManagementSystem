using Microsoft.AspNetCore.Mvc;
using TMS.TaskService.Data;

namespace TMS.TaskService.Extensions.Endpoints.Tasks;

/// <summary>
/// 
/// </summary>
public static class ReadTaskOperations
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="endpoints"></param>
    public static void AddReadTaskOperations(this IEndpointRouteBuilder endpoints)
    {
        AddGetTasksOperation(endpoints);
        AddGetTaskOperation(endpoints);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="endpoints"></param>
    private static void AddGetTasksOperation(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/tasks", async (
            [FromServices] ILogger<IApplicationBuilder> logger, 
            [FromServices] ITaskRepository repository) =>
        {
            var tasks = await repository.GetAllAsync();
            return Results.Ok(tasks);
        });
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="endpoints"></param>
    private static void AddGetTaskOperation(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/tasks/{id}", async (
            [FromRoute] int id,
            [FromServices] ILogger<IApplicationBuilder> logger,
            [FromServices] ITaskRepository repository) =>
        {
            var task = await repository.GetByIdAsync(id);
            return Results.Ok(task);
        });
    }
}