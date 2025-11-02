using Microsoft.AspNetCore.Mvc;
using TMS.TaskService.Data;

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
            try
            {
                await repository.DeleteAsync(id);
            }
            catch (KeyNotFoundException knf)
            {
                logger.LogError(knf, "AddDeleteTaskOperation");
                return Results.NotFound(knf.Message);
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "AddDeleteTaskOperations");
                Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }

            return Results.NoContent();
        });
    }
}