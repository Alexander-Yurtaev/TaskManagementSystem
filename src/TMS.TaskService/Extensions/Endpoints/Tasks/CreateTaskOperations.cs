using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using TMS.TaskService.Data;
using TMS.TaskService.Entities;
using TMS.TaskService.Models.Tasks;

namespace TMS.TaskService.Extensions.Endpoints.Tasks;

/// <summary>
/// 
/// </summary>
public static class CreateTaskOperations
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="endpoints"></param>
    public static void AddCreateTaskOperations(this IEndpointRouteBuilder endpoints)
    {
        AddCreateTaskOperation(endpoints);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="endpoints"></param>
    private static void AddCreateTaskOperation(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/tasks", async (
            [FromBody] TaskCreate task,
            [FromServices] ILogger<IApplicationBuilder> logger,
            [FromServices] IMapper mapper,
            [FromServices] ITaskRepository repository) =>
        {
            var taskExists = await repository.IsExistsAsync(task.Title);
            if (taskExists)
            {
                Results.BadRequest($"Task with title='{task.Title}' already exists.");
            }

            var entity = mapper.Map<TaskEntity>(task);

            await repository.AddAsync(entity);

            return Results.Created($"api/tasks/{entity.Id}", entity);
        });
    }
}