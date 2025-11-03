using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using TMS.TaskService.Data.Repositories;
using TMS.TaskService.Models.Tasks;

namespace TMS.TaskService.Extensions.Endpoints.Tasks;

/// <summary>
/// 
/// </summary>
public static class UpdateTaskOperations
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="endpoints"></param>
    public static void AddUpdateTaskOperations(this IEndpointRouteBuilder endpoints)
    {
        AddUpdateTaskOperation(endpoints);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="endpoints"></param>
    private static void AddUpdateTaskOperation(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut("/tasks/{id}", async (
            [FromRoute] int id,
            [FromBody] TaskUpdate taskUpdate,
            [FromServices] ILogger<IApplicationBuilder> logger,
            [FromServices] IMapper mapper,
            [FromServices] ITaskRepository repository) =>
        {
            logger.LogInformation("Start updating task with id: {Id}.", id);

            try
            {
                var task = await repository.GetByIdAsync(id);

                if (task is null)
                {
                    logger.LogWarning("Task not found with id: {Id}.", id);

                    return Results.BadRequest($"Task with id={id} does not exists.");
                }

                mapper.Map(taskUpdate, task);

                task = await repository.UpdateAsync(task);

                return Results.Ok(task);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Error while updating task with Name: {TaskId}. Operation: {Operation}",
                    id,
                    $"PUT /tasks/{id}"
                );

                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }
        });
    }
}