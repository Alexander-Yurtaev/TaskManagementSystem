using Microsoft.AspNetCore.Mvc;
using TMS.TaskService.Data.Repositories;

namespace TMS.TaskService.Extensions.Endpoints.Tasks;

/// <summary>
/// 
/// </summary>
public static class DeleteTaskOperations
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="endpoints"></param>
    public static void AddDeleteTaskOperations(this IEndpointRouteBuilder endpoints)
    {
        AddDeleteTaskOperation(endpoints);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="endpoints"></param>
    private static void AddDeleteTaskOperation(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapDelete("/tasks/{id}", async (
            [FromRoute] int id,
            [FromServices] ILogger<IApplicationBuilder> logger,
            [FromServices] ITaskRepository repository) =>
        {
            logger.LogInformation("Start deleting task with id: {Id}.", id);

            try
            {
                await repository.DeleteAsync(id);

                logger.LogInformation("Finish delete task with id={Id}.", id);

                return Results.NoContent();
            }
            catch (KeyNotFoundException knf)
            {
                logger.LogError(
                    knf,
                    "Task not found with Id: {TaskId}. Operation: {Operation}",
                    id,
                    "DELETE /tasks"
                );

                return Results.NotFound(knf.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Error while deleting task with Id: {TaskId}. Operation: {Operation}",
                    id,
                    "DELETE /tasks"
                );

                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }
        });
    }
}