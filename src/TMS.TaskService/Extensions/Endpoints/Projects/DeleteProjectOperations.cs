using Microsoft.AspNetCore.Mvc;
using TMS.TaskService.Data;

namespace TMS.TaskService.Extensions.Endpoints.Projects;

/// <summary>
/// 
/// </summary>
public static class DeleteProjectOperations
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="endpoints"></param>
    public static void AddDeleteProjectOperations(this IEndpointRouteBuilder endpoints)
    {
        AddDeleteProjectOperation(endpoints);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="endpoints"></param>
    private static void AddDeleteProjectOperation(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapDelete("/projects/{id}", async (
            [FromRoute] int id,
            [FromServices] ILogger<IApplicationBuilder> logger,
            [FromServices] IProjectRepository repository) =>
        {
            try
            {
                await repository.DeleteAsync(id);
            }
            catch (KeyNotFoundException knf)
            {
                logger.LogError(knf, "AddDeleteProjectOperation");
                return Results.NotFound(knf.Message);
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "AddDeleteProjectOperation");
                Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }

            return Results.NoContent();
        });
    }
}